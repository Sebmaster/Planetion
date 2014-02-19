using System;
using OpenTK;

namespace Planetion.Utils {
    public static class GeometryHelpers {

        [Flags]
        public enum TransformSetters {
            NONE = 0,
            TRANSLATE = 1,
            ROTATE = 2,
            SCALE = 4,
            ALL = 7
        }

        public static void Recompose(ref Matrix4 mat, Planetion.Objects.Transform trans, TransformSetters setters) {
            if (setters == TransformSetters.NONE) {
                mat = Matrix4.Identity;
                return;
            }

            if ((setters & TransformSetters.TRANSLATE) != TransformSetters.TRANSLATE) {
                mat.M41 = 0;
                mat.M42 = 0;
                mat.M43 = 0;
            }

            do {
                if ((setters & TransformSetters.ROTATE) != TransformSetters.ROTATE) {
                    Matrix4 temp = Matrix4.CreateFromQuaternion(trans.Rotation);
                    temp.Invert();
                    Matrix4.Mult(ref mat, ref temp, out mat);
                }

                if ((setters & TransformSetters.SCALE) != TransformSetters.SCALE) {
                    Matrix4 temp = Matrix4.CreateScale(trans.Scale);
                    temp.Invert();
                    float x = mat.M41, y = mat.M42, z = mat.M43;
                    Matrix4.Mult(ref mat, ref temp, out mat);
                    mat.M41 = x;
                    mat.M42 = y;
                    mat.M43 = z;
                }

                trans = trans.Parent;
            } while (trans != null);
        }
    }
}
