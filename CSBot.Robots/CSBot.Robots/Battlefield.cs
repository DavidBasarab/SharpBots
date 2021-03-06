﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace CSBot.Robots
{
    public class Battlefield
    {

        public Battlefield(int width, int height, int timeout, long seed)
        {
            Width = width;
            Height = height;
            Seed = seed;
            Time = 0;
            _robots = new List<RobotRunner>();
            //@teams = Hash.new{|h,k| h[k] = [] }
            _bullets = new List<Bullet>();
            _explosions = new List<Explosion>();
            _newBullets = new List<Bullet>();
            _newExplosions = new List<Explosion>();
            Timeout = timeout;
            GameOver = false;
            _random = new Random((int)seed);
        }

        public int Teams { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public List<RobotRunner> Robots
        {
            get { return _robots; }
        }

        public List<Bullet> Bullets
        {
            get { return _bullets; }
        }

        public List<Bullet> NewBullets
        {
            get { return _newBullets; }
        }

        public List<Explosion> Explosions
        {
            get { return _explosions; }
        }

        public List<Explosion> NewExplosions
        {
            get { return _newExplosions; }
        }

        public int Time { get; set; }
        public long Seed { get; set; }
        public int Timeout { get; set; }
        public bool GameOver { get; set; }

        public void Add(RobotRunner robot)
        {
            _robots.Add(robot);
        }

        internal void Add(Bullet bullet)
        {
            _newBullets.Add(bullet);
        }

        public void Process(Bullet bullet)
        {
            _newBullets.Remove(bullet);
            _bullets.Add(bullet);
        }

        internal void Add(Explosion explosion)
        {
            _newExplosions.Add(explosion);
        }

        public void Process(Explosion explosion)
        {
            _newExplosions.Remove(explosion);
            _explosions.Add(explosion);
        }

        private readonly List<RobotRunner> _robots = new List<RobotRunner>();
        private readonly List<Bullet> _bullets = new List<Bullet>();
        private readonly List<Bullet> _newBullets = new List<Bullet>();
        private readonly List<Explosion> _explosions = new List<Explosion>();
        private readonly List<Explosion> _newExplosions = new List<Explosion>();
        //attr_reader :teams

        private readonly Random _random;
        public Random Random { get { return _random; } }

        public bool Tick()
        {
            HandleExplosions();
            HandleBullets();
            foreach (var bullet in _bullets)
            {
                bullet.Tick();
            }

            foreach (var robot in _robots)
            {
                try
                {
                    if (!robot.Dead()) robot.InternalTick();
                }
                catch (Exception bang)
                {
                    Console.WriteLine(robot.Name + "made an exception:");
                    Console.WriteLine(bang.ToString());
                    robot.Energy = -1;
                }
            }

            Time += 1;
            var liveRobots = _robots.Where(robot => !robot.Dead()).ToList();

            var multipleTeamsLive = false;
            if (liveRobots.Count > 0)
            {
                var firstTeam = liveRobots[0].Team;
                foreach (var robot in liveRobots.Where(robot => robot.Team != firstTeam))
                {
                    multipleTeamsLive = true;
                }
            }
            GameOver = ((Time >= Timeout) ||
                        (liveRobots.Count == 0) || (multipleTeamsLive == false));
            return !GameOver;
        }

        private void HandleExplosions()
        {
            var deletions = _explosions.Where(explosion => explosion.Dead).ToList();
            foreach (var explosion in deletions) _explosions.Remove(explosion);
            foreach (var explosion in _explosions) explosion.Tick();
        }

        private void HandleBullets()
        {
            var deletions = _bullets.Where(bullet => bullet.Dead).ToList();
            foreach (var bullet in deletions) _bullets.Remove(bullet);
        }
    }
}
