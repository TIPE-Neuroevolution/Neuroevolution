﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Neuroevolution
{
    public struct DistanceJointStruct
    {
        public int a;
        public int b;

        public DistanceJointStruct(int a, int b)
        {
            this.a = a;
            this.b = b;
        }
    }

    public struct RevoluteJointStruct
    {
        public int a;
        public int b;
        public int anchor;
        public float lowerLimit;
        public float upperLimit;
        public float speed;

        public RevoluteJointStruct (int a, int b, int anchor, float lowerLimit, float upperLimit, float speed)
        {
            this.a = a;
            this.b = b;
            this.anchor = anchor;
            this.lowerLimit = lowerLimit;
            this.upperLimit = upperLimit;
            this.speed = speed;
        }
    }
}