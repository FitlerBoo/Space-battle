﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Space_battle.Model
{
    internal class Player : UserGameObject
    {
        public new Rect HitBox => new Rect((X + 15), (Y + 15), 30, 30); 
        public string StringHP => "Player HP : " + HP; 

        public Player()
        {
            SetForm();

            // TODO: Вынести в конструктор базового класса
            angleStartPosition = -90;
        }

        public Player(double x, double y, double angle, double HP)
        {
            SetForm();

            // TODO: Вынести в конструктор базового класса
            X = x;
            Y = y;
            Angle = angle + 18;
            this.HP = HP;
            Transform();
        }

        private void SetForm()
        {
            Form = new Rectangle()
            {
                Width = 60,
                Height = 60,
                Fill = new ImageBrush(new BitmapImage(new Uri(@"..\..\Images\vehicleU.png", UriKind.Relative)))
            };
        }

        public Bullet MakeBullet()
        {
            return new Bullet(this);
        }
    }
}
