using System;

namespace CommonLib.Util.Math
{
    public static class GMath
    {
        public static float Clamp(float min, float max, float val)
        {
            return val < min ? min : val > max ? max : val;
        }
    }

    public struct Vec2
    {
        public static readonly ushort INVALID_AXIS = UInt16.MaxValue;

        public static readonly Vec2 ZERO = new Vec2(0, 0);
        public static readonly Vec2 INVALID = new Vec2(INVALID_AXIS, INVALID_AXIS);

        public static readonly Vec2 UP = new Vec2(0, 1);
        public static readonly Vec2 RIGHT = new Vec2(1, 0);
        public static readonly Vec2 DOWN = new Vec2(0, -1);
        public static readonly Vec2 LEFT = new Vec2(-1, 0);

        public static readonly Vec2[] ALL_DIRS = new Vec2[] { UP, RIGHT, DOWN, LEFT };
        public static readonly Vec2[][] RND_DIRS = new Vec2[][]
        {
            new Vec2[]{ UP, RIGHT, DOWN, LEFT },
            new Vec2[]{ LEFT, UP, RIGHT, DOWN },
            new Vec2[]{ RIGHT, DOWN, LEFT, UP },
            new Vec2[]{ DOWN, LEFT, UP, RIGHT },
        };

        public ushort x;
        public ushort y;

        public Vec2(int x, int y)
        {
            this.x = (ushort)x;
            this.y = (ushort)y;
        }

        public bool IsValid()
        {
            return x != INVALID_AXIS && y != INVALID_AXIS;
        }

        public bool IsOnBounds(int boundX, int boundY)
        {
            return x < boundX && y < boundY;
        }

        public float Magnitude()
        {
            return (float)System.Math.Sqrt(x * x + y * y);
        }

        public Vec2f Normalize()
        {
            var mag = Magnitude();
            return new Vec2f(x / mag, y / mag);
        }

        public override bool Equals(object o)
        {
            if (o == null || !(o is Vec2))
                return false;

            return this == (Vec2)o;
        }

        public bool Equals(Vec2 v)
        {
            return (x == v.x) && (y == v.y);
        }

        public override int GetHashCode()
        {
            return (x ^ 7) ^ (y ^ 13);
        }

        public int Distance(Vec2 other)
        {
            var distance = (Vec2i)this - (Vec2i)other;
            return (int)distance.Magnitude();
        }

        public static bool operator ==(Vec2 lhs, Vec2 rhs)
        {
            return lhs.x == rhs.x && lhs.y == rhs.y;
        }

        public static bool operator !=(Vec2 lhs, Vec2 rhs)
        {
            return !(lhs == rhs);
        }

        public static Vec2 operator -(Vec2 lhs, Vec2 rhs)
        {
            return new Vec2(lhs.x - rhs.x, lhs.y - rhs.y);
        }

        public static Vec2 operator +(Vec2 lhs, Vec2 rhs)
        {
            return new Vec2(lhs.x + rhs.x, lhs.y + rhs.y);
        }

        public static Vec2 operator *(Vec2 v, int i)
        {
            return new Vec2(v.x * i, v.y * i);
        }

        public override string ToString()
        {
            return "[" + x + "," + y + "]";
        }

        public static implicit operator Vec2(Vec2f other)
        {
            return new Vec2((int)other.x, (int)other.y);
        }

        public static implicit operator Vec2(Vec2i other)
        {
            return new Vec2((int)other.x, (int)other.y);
        }
    }

    public struct Vec2i
    {
        public static readonly short INVALID_AXIS = Int16.MaxValue;

        public static readonly Vec2i ZERO = new Vec2i(0, 0);
        public static readonly Vec2i INVALID = new Vec2i(INVALID_AXIS, INVALID_AXIS);

        public short x;
        public short y;

        public Vec2i(int x, int y)
        {
            this.x = (short)x;
            this.y = (short)y;
        }

        public bool IsValid()
        {
            return x != INVALID_AXIS && y != INVALID_AXIS;
        }

        public bool IsOnBounds(int boundX, int boundY)
        {
            return x < boundX && y < boundY;
        }

        public float Magnitude()
        {
            return (float)System.Math.Sqrt(x * x + y * y);
        }

        public Vec2f Normalize()
        {
            var mag = Magnitude();
            return new Vec2f(x / mag, y / mag);
        }

        public override bool Equals(object o)
        {
            if (o == null || !(o is Vec2i))
                return false;

            return this == (Vec2i)o;
        }

        public bool Equals(Vec2i v)
        {
            return (x == v.x) && (y == v.y);
        }

        public override int GetHashCode()
        {
            return (x ^ 7) ^ (y ^ 13);
        }

        public static bool operator ==(Vec2i lhs, Vec2i rhs)
        {
            return lhs.x == rhs.x && lhs.y == rhs.y;
        }

        public static bool operator !=(Vec2i lhs, Vec2i rhs)
        {
            return !(lhs == rhs);
        }

        public static Vec2i operator -(Vec2i lhs, Vec2i rhs)
        {
            return new Vec2i(lhs.x - rhs.x, lhs.y - rhs.y);
        }

        public static Vec2i operator +(Vec2i lhs, Vec2i rhs)
        {
            return new Vec2i(lhs.x + rhs.x, lhs.y + rhs.y);
        }

        public override string ToString()
        {
            return "[" + x + "," + y + "]";
        }

        public static implicit operator Vec2i(Vec2f other)
        {
            return new Vec2i((short)other.x, (short)other.y);
        }

        public static implicit operator Vec2i(Vec2 other)
        {
            return new Vec2i(other.x, other.y);
        }
    }

    public struct Vec2f
    {
        public static readonly float EPSILON = 0.00001f;
        public static readonly float INVALID_AXIS = float.NaN;

        public static readonly Vec2f ZERO = new Vec2f(0, 0);
        public static readonly Vec2f INVALID = new Vec2f(INVALID_AXIS, INVALID_AXIS);

        public float x;
        public float y;

        public Vec2f(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public bool IsValid()
        {
            return !float.IsNaN(x) && !float.IsNaN(y);
        }

        public bool IsOnBounds(float boundX, float boundY)
        {
            return x < boundX && y < boundY;
        }

        public float Magnitude()
        {
            return (float)System.Math.Sqrt(x * x + y * y);
        }

        public void Normalize()
        {
            var mag = Magnitude();
            x /= mag;
            y /= mag;
        }

        public void Reset()
        {
            x = 0;
            y = 0;
        }

        public void Clamp(float min, float max)
        {
            x = GMath.Clamp(min, max, x);
            y = GMath.Clamp(min, max, y);
        }

        public void Clamp(Vec2f min, Vec2f max)
        {
            x = GMath.Clamp(min.x, max.x, x);
            y = GMath.Clamp(min.y, max.y, y);
        }

        public static Vec2f operator +(Vec2f lhs, int rhs)
        {
            return new Vec2f(lhs.x + rhs, lhs.y + rhs);
        }

        public static bool operator ==(Vec2f lhs, Vec2f rhs)
        {
            return System.Math.Abs(lhs.x - rhs.x) < EPSILON && System.Math.Abs(lhs.y - rhs.y) < EPSILON;
        }

        public static bool operator !=(Vec2f lhs, Vec2f rhs)
        {
            return !(lhs == rhs);
        }

        public static Vec2f operator -(Vec2f lhs, Vec2f rhs)
        {
            return new Vec2f(lhs.x - rhs.x, lhs.y - rhs.y);
        }

        public static Vec2f operator +(Vec2f lhs, Vec2f rhs)
        {
            return new Vec2f(lhs.x + rhs.x, lhs.y + rhs.y);
        }

        public static Vec2f operator +(Vec2f lhs, float rhs)
        {
            return new Vec2f(lhs.x + rhs, lhs.y + rhs);
        }

        public static Vec2f operator -(Vec2f lhs, float rhs)
        {
            return new Vec2f(lhs.x - rhs, lhs.y - rhs);
        }

        public static Vec2f operator *(Vec2f lhs, float rhs)
        {
            return new Vec2f(lhs.x * rhs, lhs.y * rhs);
        }

        public override bool Equals(object o)
        {
            if (o == null || !(o is Vec2f))
                return false;

            return this == (Vec2f)o;
        }

        public bool Equals(Vec2f v)
        {
            return (x == v.x) && (y == v.y);
        }
        public override int GetHashCode()
        {
            return (int)(System.Math.Sqrt((int)(x * 1000) ^ 7)) ^ (int)(System.Math.Sqrt((int)(y * 1000) ^ 13));
        }

        public override string ToString()
        {
            return "[" + x + "," + y + "]";
        }

        public static implicit operator Vec2f(Vec2i other)
        {
            return new Vec2f(other.x, other.y);
        }

        public static implicit operator Vec2f(Vec2 other)
        {
            return new Vec2f(other.x, other.y);
        }
    }

    public struct Rang2
    {
        public static readonly Rang2 INVALID = new Rang2(Vec2.INVALID, Vec2.INVALID);

        public Vec2 beg;
        public Vec2 end;

        public Rang2(Vec2 beg, Vec2 end)
        {
            this.beg = beg;
            this.end = end;
        }

        public bool IsValid()
        {
            return beg.IsValid() && end.IsValid();
        }
    }


}
