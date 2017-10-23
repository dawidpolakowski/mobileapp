using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Realms;
using Toggl.Multivac;

namespace Toggl.PrimeRadiant.Realm
{
    internal interface IRealmAdapter<TModel>
    {
        IQueryable<TModel> GetAll();

        TModel Get(long id);

        void Delete(long id);

        TModel Create(TModel entity);

        TModel Update(long id, TModel entity);

        IEnumerable<IConflictResolutionResult<TModel>> BatchUpdate(
            IEnumerable<(long Id, TModel Entity)> batch,
            Func<TModel, TModel, ConflictResolutionMode> conflictResolution,
            IRivalsResolver<TModel> rivalsResolver);
    }

    internal sealed class RealmAdapter<TRealmEntity, TModel> : IRealmAdapter<TModel>
        where TRealmEntity : RealmObject, TModel, IUpdatesFrom<TModel>
    {
        private readonly Func<TModel, Realms.Realm, TRealmEntity> clone;

        private readonly Func<long, Expression<Func<TRealmEntity, bool>>> matchEntity;

        private readonly Func<TRealmEntity, long> getId;

        public RealmAdapter(
            Func<TModel, Realms.Realm, TRealmEntity> clone,
            Func<long, Expression<Func<TRealmEntity, bool>>> matchEntity,
            Func<TRealmEntity, long> getId)
        {
            Ensure.Argument.IsNotNull(clone, nameof(clone));
            Ensure.Argument.IsNotNull(matchEntity, nameof(matchEntity));
            Ensure.Argument.IsNotNull(getId, nameof(getId));

            this.clone = clone;
            this.matchEntity = matchEntity;
            this.getId = getId;
        }

        public IQueryable<TModel> GetAll()
            => Realms.Realm.GetInstance().All<TRealmEntity>();

        public TModel Get(long id)
            => Realms.Realm.GetInstance().All<TRealmEntity>().Single(matchEntity(id));

        public TModel Create(TModel entity)
        {
            Ensure.Argument.IsNotNull(entity, nameof(entity));

            return doTransaction(realm => realm.Add(convertToRealm(entity, realm)));
        }
        
        public TModel Update(long id, TModel entity)
        {
            Ensure.Argument.IsNotNull(entity, nameof(entity));

            return doModyfingTransaction(id, (realm, realmEntity) => realmEntity.SetPropertiesFrom(entity, realm));
        }

        public IEnumerable<IConflictResolutionResult<TModel>> BatchUpdate(
            IEnumerable<(long Id, TModel Entity)> batch,
            Func<TModel, TModel, ConflictResolutionMode> conflictResolution,
            IRivalsResolver<TModel> rivalsResolver)
        {
            Ensure.Argument.IsNotNull(batch, nameof(batch));
            Ensure.Argument.IsNotNull(matchEntity, nameof(matchEntity));
            Ensure.Argument.IsNotNull(conflictResolution, nameof(conflictResolution));

            var realm = Realms.Realm.GetInstance();
            using (var transaction = realm.BeginWrite())
            {
                var realmEntities = realm.All<TRealmEntity>();
                var entitiesWithPotentialRival = new List<TRealmEntity>();
                var results = batch.Select(updated =>
                {
                    var oldEntity = realmEntities.SingleOrDefault(matchEntity(updated.Id));
                    var resolveMode = conflictResolution(oldEntity, updated.Entity);
                    var result = resolveEntity(realm, updated.Id, oldEntity, updated.Entity, resolveMode);

                    if (rivalsResolver != null &&
                        (result is CreateResult<TModel> || result is UpdateResult<TModel>))
                    {
                        var resolvedEntity = getEntityFromResult(result);
                        if (rivalsResolver.CanHaveRival(resolvedEntity))
                            entitiesWithPotentialRival.Add(resolvedEntity);
                    }

                    return result;
                }).ToList();

                foreach (var entityWithPotentialRival in entitiesWithPotentialRival)
                {
                    resolvePotentialRivals(realm, entityWithPotentialRival, rivalsResolver, results);
                }

                transaction.Commit();
                return results;
            }
        }

        public void Delete(long id)
        {
            doModyfingTransaction(id, (realm, realmEntity) => realm.Remove(realmEntity));
        }

        private TRealmEntity convertToRealm(TModel entity, Realms.Realm realm)
            => entity as TRealmEntity ?? clone(entity, realm);

        private TModel doModyfingTransaction(long id, Action<Realms.Realm, TRealmEntity> transact)
            => doTransaction(realm =>
            {
                var realmEntity = realm.All<TRealmEntity>().Single(matchEntity(id));
                transact(realm, realmEntity);
                return realmEntity;
            });

        private TModel doTransaction(Func<Realms.Realm, TModel> transact)
        {
            var realm = Realms.Realm.GetInstance();
            using (var transaction = realm.BeginWrite())
            {
                var returnValue = transact(realm);
                transaction.Commit();
                return returnValue;
            }
        }

        private IConflictResolutionResult<TModel> resolveEntity(Realms.Realm realm, long oldId, TRealmEntity old, TModel entity, ConflictResolutionMode resolveMode)
        {
            switch (resolveMode)
            {
                case ConflictResolutionMode.Create:
                    var realmEntity = realm.Add(convertToRealm(entity, realm));
                    return new CreateResult<TModel>(realmEntity);

                case ConflictResolutionMode.Delete:
                    realm.Remove(old);
                    return new DeleteResult<TModel>(oldId);

                case ConflictResolutionMode.Update:
                    old.SetPropertiesFrom(entity, realm);
                    return new UpdateResult<TModel>(oldId, old);

                case ConflictResolutionMode.Ignore:
                    return new IgnoreResult<TModel>(oldId);

                default:
                    throw new ArgumentException($"Unknown conflict resolution mode {resolveMode}");
            }
        }

        private TRealmEntity getEntityFromResult(IConflictResolutionResult<TModel> result)
        {
            switch (result)
            {
                case CreateResult<TModel> c:
                    return (TRealmEntity)c.Entity;
                case UpdateResult<TModel> u:
                    return (TRealmEntity)u.Entity;
                default:
                    throw new ArgumentOutOfRangeException(nameof(result));
            }
        }

        private void resolvePotentialRivals(
            Realms.Realm realm,
            TRealmEntity entity,
            IRivalsResolver<TModel> resolver,
            List<IConflictResolutionResult<TModel>> results)
        {
            var rival = (TRealmEntity)realm.All<TRealmEntity>().SingleOrDefault(resolver.AreRivals(entity));
            if (rival != null)
            {
                long originalRivalId = getId(rival);
                (TModel fixedEntity, TModel fixedRival) = resolver.FixRivals(entity, rival, realm.All<TRealmEntity>());
                entity.SetPropertiesFrom(fixedEntity, realm);
                rival.SetPropertiesFrom(fixedRival, realm);
                results.Add(new UpdateResult<TModel>(originalRivalId, rival));
            }
        }
    }
}
