
/*
 * Based on:
 * https://github.com/craftworkgames/MonoGame.Extended/blob/7c8de64caa135eaea39885a99e0e8e706a85bfad/Source/MonoGame.Extended/Math/Matrix2.cs
 * https://github.com/craftworkgames/MonoGame.Extended/blob/7c8de64caa135eaea39885a99e0e8e706a85bfad/Source/MonoGame.Extended/Shapes/Polygon.cs
 *
 * Represents a polygon that is transformable using a transform Matrix.
 * 
 * Copyright 2020 Ashley Horton
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
 * documentation files (the "Software"), to deal in the Software without restriction, including without limitation the
 * rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the
 * Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
 * WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
 * COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
 * OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System.Linq;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace utils
{
    public struct OrientedPolygon
    {
        private readonly Vector2[] _localBounds;
        private Vector2[] _transformedVertices;
        private Matrix2 _transform;
        private bool _dirty;

        public Vector2[] LocalVertices => _localBounds;

        public RectangleF BoundingRectangle
        {
            get
            {
                var minX = Left;
                var minY = Top;
                var maxX = Right;
                var maxY = Bottom;
                return new RectangleF(minX, minY, maxX - minX, maxY - minY);
            }
        }

        public Vector2[] TransformedVertices
        {
            get
            {
                if (_dirty)
                {
                    UpdateTransformedVertices();
                    _dirty = false;
                }

                return _transformedVertices;
            }
        }

        public float Left
        {
            get { return TransformedVertices.Min(v => v.X); }
        }

        public float Right
        {
            get { return TransformedVertices.Max(v => v.X); }
        }

        public float Top
        {
            get { return TransformedVertices.Min(v => v.Y); }
        }

        public float Bottom
        {
            get { return TransformedVertices.Max(v => v.Y); }
        }

        private void UpdateTransformedVertices()
        {
            for (var i = 0; i < _localBounds.Length; i++)
            {
                _transformedVertices[i] = _transform.Transform(_localBounds[i]);
            }
        }

        public void Translate(Vector2 translation)
        {
            _transform = _transform * Matrix2.CreateTranslation(translation);
            _dirty = true;
        }

        public void Rotate(float radians)
        {
            _transform = _transform * Matrix2.CreateRotationZ(radians);
            _dirty = true;
        }

        public void Scale(Vector2 scale)
        {
            _transform = _transform * Matrix2.CreateScale(scale);
            _dirty = true;
        }

        public OrientedPolygon TransformedCopy(Vector2 translation, float rotation, Vector2 scale) =>
            new OrientedPolygon(_localBounds, Matrix2.CreateFrom(translation, rotation, scale));

        public OrientedPolygon(Vector2[] vertices, Matrix2 transform)
        {
            _localBounds = vertices;
            _transform = transform;
            _transformedVertices = new Vector2[vertices.Length];
            _dirty = false;

            UpdateTransformedVertices();
        }

        public bool Contains(Vector2 point)
        {
            return Contains(point.X, point.Y);
        }

        public bool Contains(float x, float y)
        {
            var intersects = 0;
            var vertices = TransformedVertices;

            for (var i = 0; i < vertices.Length; i++)
            {
                var x1 = vertices[i].X;
                var y1 = vertices[i].Y;
                var x2 = vertices[(i + 1) % vertices.Length].X;
                var y2 = vertices[(i + 1) % vertices.Length].Y;

                if ((((y1 <= y) && (y < y2)) || ((y2 <= y) && (y < y1))) && (x < (x2 - x1) / (y2 - y1) * (y - y1) + x1))
                    intersects++;
            }

            return (intersects & 1) == 1;
        }

        public static bool operator ==(OrientedPolygon a, OrientedPolygon b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(OrientedPolygon a, OrientedPolygon b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is OrientedPolygon && Equals((OrientedPolygon) obj);
        }

        public bool Equals(OrientedPolygon other)
        {
            return TransformedVertices.SequenceEqual(other.TransformedVertices);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return TransformedVertices.Aggregate(27, (current, v) => current + 13 * current + v.GetHashCode());
            }
        }
    }
}