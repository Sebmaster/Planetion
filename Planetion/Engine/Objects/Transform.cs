using System;
using OpenTK;
using Planetion.Utils;

namespace Planetion.Objects {
    public class Transform {

        public event EventHandler TransformUpdate;

        private bool dirty = false;

        private bool Dirty {
            get { return dirty; }

            set {
                dirty = true;

                if (TransformUpdate != null)
                    TransformUpdate(this, new EventArgs());
            }
        }

        private Transform parent;
        public Transform Parent {
            get { return parent; }
            set {
                if (parent != null) {
                    parent.GameObject.Children.Remove(GameObject);
                }

                parent = value;
                if (parent != null) {
                    parent.GameObject.Children.Add(GameObject);
                }

                Dirty = true;
            }
        }

        public GameObject GameObject;

        private Vector3 localPosition = Vector3.Zero;
        /// <summary>
        /// The position of the object in 3d space relative to its parent.
        /// </summary>
        public Vector3 LocalPosition {
            get {
                return localPosition;
            }
            set {
                if (localPosition != value) {
                    localPosition = value;
                    Dirty = true;
                }
            }
        }

        private Quaternion rotation = Quaternion.Identity;
        /// <summary>
        /// The rotation of the object.
        /// </summary>
        public Quaternion Rotation {
            get { return rotation; }
            set {
                if (rotation != value) {
                    rotation = value;
                    Dirty = true;
                }
            }
        }

        private Vector3 scale = Vector3.One;
        /// <summary>
        /// The scale of the object.
        /// </summary>
        public Vector3 Scale {
            get { return scale; }
            set {
                if (scale != value) {
                    scale = value;
                    Dirty = true;
                }
            }
        }

        /// <summary>
        /// Returns the position of the object in global coordinates.
        /// </summary>
        public Vector3 Position {
            get {
                Matrix4 mat = Model;

                return new Vector3(mat.M41, mat.M42, mat.M43);
            }

            set {
                if (Parent != null) {
                    Matrix4 mat = Parent.Model;

                    Quaternion rot = Parent.Rotation;
                    Vector3.Transform(ref value, ref rot, out value);

                    LocalPosition = new Vector3(value.X - mat.M41, value.Y - mat.M42, value.Z - mat.M43);
                } else {
                    LocalPosition = value;
                }
            }
        }

        private Matrix4 model = Matrix4.Identity;
        public Matrix4 Model {
            get {
                if (!Dirty) return model;

                Matrix4 mat = Matrix4.Identity;
                Transform obj = this;

                do {
                    Matrix4 scale = Matrix4.CreateScale(obj.Scale);
                    Matrix4.Mult(ref mat, ref scale, out mat);
                    Matrix4 rot = Matrix4.CreateFromQuaternion(obj.Rotation);
                    Matrix4.Mult(ref mat, ref rot, out mat);

                    mat.M41 = obj.LocalPosition.X;
                    mat.M42 = obj.LocalPosition.Y;
                    mat.M43 = obj.LocalPosition.Z;

                    obj = obj.Parent;
                } while (obj != null);

                Dirty = false;
                return model = mat;
            }
        }

        public Transform(GameObject obj) {
            GameObject = obj;
        }

        /// <summary>
        /// Moves the game object by the defined x, y and z coordinates.
        /// </summary>
        /// <param name='x'>the x units</param>
        /// <param name='y'>the y units</param>
        /// <param name='z'>the z units</param>
        public void Translate(float x, float y, float z) {
            localPosition.X += x;
            localPosition.Y += y;
            localPosition.Z += z;
            Dirty = true;
        }

        /// <summary>
        /// Rotate around the specified axis and angle.
        /// </summary>
        /// <param name='axis'>the axis to rotate around</param>
        /// <param name='angle'>the angle in degrees</param>
        public void Rotate(Vector3 axis, float angle) {
            Rotation *= Quaternion.FromAxisAngle(axis, MathHelper.DegreesToRadians(angle));
        }
    }
}

