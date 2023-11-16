using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Portfolio.Entities;
using Portfolio.Gameplay.Components;
using Portfolio.Gameplay.Systems;

namespace Portfolio.Gameplay;

public class Game
{
    private readonly World _world;
    private readonly ISystem[] _systems;

    private readonly Dictionary<int, Entity> _characters = new();
    private readonly Stopwatch _stopwatch = new();
    private readonly ReaderWriterLockSlim _lock = new();

    public Game()
    {
        _world = new World();

        _systems = new ISystem[]
        {
            new TranslationSystem(_world)
        };
    }

    public void Update()
    {
        try
        {
            _lock.EnterWriteLock();

            var delta = (float) _stopwatch.Elapsed.TotalSeconds;
            _stopwatch.Restart();

            for (var i = 0; i < _systems.Length; i++)
            {
                _systems[i].Tick(delta);
            }
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void SpawnPlayerCharacter(int peer)
    {
        try
        {
            _lock.EnterWriteLock();

            var entity = _world.Create();
            _world.SetComponent(entity, new Position());
            _world.SetComponent(entity, new Velocity());
            _characters[peer] = entity;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public Entity GetPlayerCharacter(int peer)
    {
        try
        {
            _lock.EnterReadLock();
            return _characters[peer];
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public void Command<TCommand>(TCommand command) where TCommand : ICommand
    {
        try
        {
            _lock.EnterWriteLock();
            command.Execute(_world);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public TResult Query<TQuery, TResult>(TQuery query) where TQuery : IQuery<TResult>
    {
        try
        {
            _lock.EnterReadLock();
            return query.Execute(_world);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }
}
