using LearnOpenTK.Common;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace Project
{
    internal class Asset3d
    {
        List<Vector3> _vertices = new List<Vector3>();
        List<uint> _indices = new List<uint>();
        int _vertexBufferObject;
        int _vertexArrayObject;
        int _elementBufferObject;
        Shader _shader;
        Matrix4 _view;
        Matrix4 _projection;
        Matrix4 _model = Matrix4.Identity;
        public Vector3 _centerPosition;
        public List<Vector3> _euler;
        public List<Asset3d> Child;
        private Vector3 color;
        public Vector3 defaultPosition;

        public Asset3d(List<Vector3> vertices, List<uint> indices)
        {
            _vertices = vertices;
            _indices = indices;
            setdefault();
        }

        public Asset3d(Vector3 _color)
        {
            this.color = _color;
            setdefault();
        }

        public Asset3d()
        {
            _vertices = new List<Vector3>();
            setdefault();
        }

        public void setdefault()
        {
            _euler = new List<Vector3>();

            //sumbu X
            _euler.Add(new Vector3(1, 0, 0));

            //sumbu y
            _euler.Add(new Vector3(0, 1, 0));

            //sumbu z
            _euler.Add(new Vector3(0, 0, 1));

            _model = Matrix4.Identity;
            _centerPosition = new Vector3(0, 0, 0);
            Child = new List<Asset3d>();
        }

        public void load(string shadervert, string shaderfrag, float Size_x, float Size_y)
        {
            //Buffer
            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Count
                * Vector3.SizeInBytes, _vertices.ToArray(), BufferUsageHint.StaticDraw);

            //VAO
            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);
            //kalau mau bikin object settingannya beda dikasih if
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float,
                false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            //ada data yang disimpan di _indices
            if (_indices.Count != 0)
            {
                _elementBufferObject = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
                GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Count
                    * sizeof(uint), _indices.ToArray(), BufferUsageHint.StaticDraw);
            }

            _shader = new Shader(shadervert, shaderfrag);
            _shader.Use();

            _view = Matrix4.CreateTranslation(0.0f, 0.0f, -3.0f);

            _projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), Size_x / (float)Size_y, 0.1f, 100.0f);
            foreach (var item in Child)
            {
                item.load(shadervert, shaderfrag, Size_x, Size_y);
            }
        }

        public void render(int _lines, double time, Matrix4 camera_view, Matrix4 camera_projection)
        {
            _shader.Use();
            GL.BindVertexArray(_vertexArrayObject);
            //_model = _model * Matrix4.CreateRotationY((float)MathHelper.DegreesToRadians(time));


            _shader.SetVector3("ourColor", color);
            _shader.SetMatrix4("model", _model);
            _shader.SetMatrix4("view", camera_view);
            _shader.SetMatrix4("projection", camera_projection);


            if (_indices.Count != 0)
            {
                GL.DrawElements(PrimitiveType.Triangles, _indices.Count, DrawElementsType.UnsignedInt, 0);
            }
            else
            {

                if (_lines == 0)
                {
                    GL.DrawArrays(PrimitiveType.Triangles, 0, _vertices.Count);
                }
                else if (_lines == 1)
                {
                    GL.DrawArrays(PrimitiveType.TriangleFan, 0, _vertices.Count);
                }
                else if (_lines == 2)
                {
                    GL.DrawArrays(PrimitiveType.Lines, 0, _vertices.Count);
                }
                else if (_lines == 3)
                {
                    GL.DrawArrays(PrimitiveType.LineStrip, 0, _vertices.Count);
                }
            }

            foreach (var item in Child)
                item.render(_lines, time, camera_view, camera_projection);
        }

        public void createTranslation(Vector3 vec3)
        {
            _model = _model * Matrix4.CreateTranslation(vec3);

            _centerPosition.X += vec3.X;
            _centerPosition.Y += vec3.Y;
            _centerPosition.Z += vec3.Z;

            foreach (var obj in Child)
                obj.createTranslation(vec3);
        }

        public void createBoxVertices(float x, float y, float z, float length)
        {
            _centerPosition.X = x;
            _centerPosition.Y = y;
            _centerPosition.Z = z;
            Vector3 temp_vector;

            //TITIK 1
            temp_vector.X = x - length / 2.0f;
            temp_vector.Y = y + length / 2.0f;
            temp_vector.Z = z - length / 2.0f;
            _vertices.Add(temp_vector);
            //TITIK 2
            temp_vector.X = x + length / 2.0f;
            temp_vector.Y = y + length / 2.0f;
            temp_vector.Z = z - length / 2.0f;
            _vertices.Add(temp_vector);
            //TITIK 3
            temp_vector.X = x - length / 2.0f;
            temp_vector.Y = y - length / 2.0f;
            temp_vector.Z = z - length / 2.0f;
            _vertices.Add(temp_vector);
            //TITIK 4
            temp_vector.X = x + length / 2.0f;
            temp_vector.Y = y - length / 2.0f;
            temp_vector.Z = z - length / 2.0f;
            _vertices.Add(temp_vector);
            //TITIK 5
            temp_vector.X = x - length / 2.0f;
            temp_vector.Y = y + length / 2.0f;
            temp_vector.Z = z + length / 2.0f;
            _vertices.Add(temp_vector);
            //TITIK 6
            temp_vector.X = x + length / 2.0f;
            temp_vector.Y = y + length / 2.0f;
            temp_vector.Z = z + length / 2.0f;
            _vertices.Add(temp_vector);
            //TITIK 7
            temp_vector.X = x - length / 2.0f;
            temp_vector.Y = y - length / 2.0f;
            temp_vector.Z = z + length / 2.0f;
            _vertices.Add(temp_vector);
            //TITIK 8
            temp_vector.X = x + length / 2.0f;
            temp_vector.Y = y - length / 2.0f;
            temp_vector.Z = z + length / 2.0f;
            _vertices.Add(temp_vector);

            _indices = new List<uint>
            {
                //SEGITIGA DEPAN 1
                0,1,2,
                //SEGITIGA DEPAN 2
                1,2,3,
                //SEGITIGA ATAS 1
                0,4,5,
                //SEGITIGA ATAS 2
                0,1,5,
                //SEGITIGA KANAN 1
                1,3,5,
                //SEGITIGA KANAN 2
                3,5,7,
                //SEGITIGA KIRI 1
                0,2,4,
                //SEGITIGA KIRI 2
                2,4,6,
                //SEGITIGA BELAKANG 1
                4,5,6,
                //SEGITIGA BELAKANG 2
                5,6,7,
                //SEGITIGA BAWAH 1
                2,3,6,
                //SEGITIGA BAWAH 2
                3,6,7
            };
        }

        public void createCuboid(float radiusX, float radiusY, float radiusZ, float x, float y, float z)
        {
            _centerPosition.X = x;
            _centerPosition.Y = y;
            _centerPosition.Z = z;
            Vector3 temp_vector;

            //TITIK 1
            temp_vector.X = x - radiusX / 2.0f;
            temp_vector.Y = y + radiusY / 2.0f;
            temp_vector.Z = z - radiusZ / 2.0f;
            _vertices.Add(temp_vector);
            //TITIK 2
            temp_vector.X = x + radiusX / 2.0f;
            temp_vector.Y = y + radiusY / 2.0f;
            temp_vector.Z = z - radiusZ / 2.0f;
            _vertices.Add(temp_vector);
            //TITIK 3
            temp_vector.X = x - radiusX / 2.0f;
            temp_vector.Y = y - radiusY / 2.0f;
            temp_vector.Z = z - radiusZ / 2.0f;
            _vertices.Add(temp_vector);
            //TITIK 4
            temp_vector.X = x + radiusX / 2.0f;
            temp_vector.Y = y - radiusY / 2.0f;
            temp_vector.Z = z - radiusZ / 2.0f;
            _vertices.Add(temp_vector);
            //TITIK 5
            temp_vector.X = x - radiusX / 2.0f;
            temp_vector.Y = y + radiusY / 2.0f;
            temp_vector.Z = z + radiusZ / 2.0f;
            _vertices.Add(temp_vector);
            //TITIK 6
            temp_vector.X = x + radiusX / 2.0f;
            temp_vector.Y = y + radiusY / 2.0f;
            temp_vector.Z = z + radiusZ / 2.0f;
            _vertices.Add(temp_vector);
            //TITIK 7
            temp_vector.X = x - radiusX / 2.0f;
            temp_vector.Y = y - radiusY / 2.0f;
            temp_vector.Z = z + radiusZ / 2.0f;
            _vertices.Add(temp_vector);
            //TITIK 8
            temp_vector.X = x + radiusX / 2.0f;
            temp_vector.Y = y - radiusY / 2.0f;
            temp_vector.Z = z + radiusZ / 2.0f;
            _vertices.Add(temp_vector);

            _indices = new List<uint>
            {
                //SEGITIGA DEPAN 1
                0,1,2,
                //SEGITIGA DEPAN 2
                1,2,3,
                //SEGITIGA ATAS 1
                0,4,5,
                //SEGITIGA ATAS 2
                0,1,5,
                //SEGITIGA KANAN 1
                1,3,5,
                //SEGITIGA KANAN 2
                3,5,7,
                //SEGITIGA KIRI 1
                0,2,4,
                //SEGITIGA KIRI 2
                2,4,6,
                //SEGITIGA BELAKANG 1
                4,5,6,
                //SEGITIGA BELAKANG 2
                5,6,7,
                //SEGITIGA BAWAH 1
                2,3,6,
                //SEGITIGA BAWAH 2
                3,6,7
            };
        }

        public void createEllipsoid(float radiusX, float radiusY, float radiusZ, float _x, float _y, float _z)
        {
            _centerPosition.X = _x;
            _centerPosition.Y = _y;
            _centerPosition.Z = _z;
            float pi = (float)Math.PI;
            Vector3 temp_vector;
            for (float u = -pi; u <= pi; u += pi / 300)
            {
                for (float v = -pi / 2; v <= pi / 2; v += pi / 300)
                {
                    temp_vector.X = _x + (float)Math.Cos(v) * (float)Math.Cos(u) * radiusX;
                    temp_vector.Y = _y + (float)Math.Cos(v) * (float)Math.Sin(u) * radiusY;
                    temp_vector.Z = _z + (float)Math.Sin(v) * radiusZ;
                    _vertices.Add(temp_vector);
                }
            }
        }

        public void createEllipsoid2(float radiusX, float radiusY, float radiusZ, float _x, float _y, float _z, int sectorCount, int stackCount)
        {
            _centerPosition.X = _x;
            _centerPosition.Y = _y;
            _centerPosition.Z = _z;

            float pi = (float)Math.PI;
            Vector3 temp_vector;
            float sectorStep = 2 * (float)Math.PI / sectorCount;
            float stackStep = (float)Math.PI / stackCount;
            float sectorAngle, StackAngle, x, y, z;

            for (int i = 0; i <= stackCount; ++i)
            {
                StackAngle = pi / 2 - i * stackStep;
                x = radiusX * (float)Math.Cos(StackAngle);
                y = radiusY * (float)Math.Cos(StackAngle);
                z = radiusZ * (float)Math.Sin(StackAngle);

                for (int j = 0; j <= sectorCount; ++j)
                {
                    sectorAngle = j * sectorStep;

                    temp_vector.X = x * (float)Math.Cos(sectorAngle) + _x;
                    temp_vector.Y = y * (float)Math.Sin(sectorAngle) + _y;
                    temp_vector.Z = z + _z;
                    _vertices.Add(temp_vector);
                }
            }

            uint k1, k2;
            for (int i = 0; i < stackCount; ++i)
            {
                k1 = (uint)(i * (sectorCount + 1));
                k2 = (uint)(k1 + sectorCount + 1);
                for (int j = 0; j < sectorCount; ++j, ++k1, ++k2)
                {
                    if (i != 0)
                    {
                        _indices.Add(k1);
                        _indices.Add(k2);
                        _indices.Add(k1 + 1);
                    }
                    if (i != (stackCount - 1))
                    {
                        _indices.Add(k1 + 1);
                        _indices.Add(k2);
                        _indices.Add(k2 + 1);
                    }
                }
            }
        }

        public void createHalfEllipsoid(float radiusX, float radiusY, float radiusZ, float _x, float _y, float _z, int sectorCount, int stackCount)
        {
            _centerPosition.X = _x;
            _centerPosition.Y = _y;
            _centerPosition.Z = _z;

            float pi = (float)Math.PI;
            Vector3 temp_vector;
            float sectorStep = 2 * (float)Math.PI / sectorCount;
            float stackStep = (float)Math.PI / stackCount;
            float sectorAngle, StackAngle, x, y, z;

            for (int i = 0; i <= stackCount; ++i)
            {
                StackAngle = pi / 2 - i * stackStep;
                x = radiusX * (float)Math.Cos(StackAngle);
                y = radiusY * (float)Math.Cos(StackAngle);
                z = radiusZ * (float)Math.Sin(StackAngle);

                for (int j = 0; j <= sectorCount / 2; ++j)
                {
                    sectorAngle = j * sectorStep;

                    temp_vector.X = x * (float)Math.Cos(sectorAngle) + _x;
                    temp_vector.Y = y * (float)Math.Sin(sectorAngle) + _y;
                    temp_vector.Z = z + _z;
                    _vertices.Add(temp_vector);
                }
            }

            uint k1, k2;
            for (int i = 0; i < stackCount; ++i)
            {
                k1 = (uint)(i * (sectorCount / 2 + 1));
                k2 = (uint)(k1 + sectorCount / 2 + 1);
                for (int j = 0; j < sectorCount / 2; ++j, ++k1, ++k2)
                {
                    if (i != 0)
                    {
                        _indices.Add(k1);
                        _indices.Add(k2);
                        _indices.Add(k1 + 1);
                    }
                    if (i != (stackCount - 1))
                    {
                        _indices.Add(k1 + 1);
                        _indices.Add(k2);
                        _indices.Add(k2 + 1);
                    }
                }
            }
        }

        public void createEllipticParaboloid(float radiusX, float radiusY, float radiusZ, float _x, float _y, float _z)
        {

            _centerPosition.X = _x;
            _centerPosition.Y = _y;
            _centerPosition.Z = _z;

            float pi = (float)Math.PI;
            Vector3 temp_vector;
            for (float v = -pi; v <= pi; v += pi / 300)
            {
                for (float u = 0; u <= 15; u += pi / 300)
                {
                    temp_vector.X = _x + v * (float)Math.Cos(u) * radiusX;
                    temp_vector.Y = _y + v * (float)Math.Sin(u) * radiusY;
                    temp_vector.Z = _z + (v * v) * radiusZ;
                    _vertices.Add(temp_vector);

                }
            }
        }

        public void createCylinder(float radius, float height, float _x, float _y, float _z)
        {
            _centerPosition.X = _x;
            _centerPosition.Y = _y;
            _centerPosition.Z = _z;
            float pi = (float)Math.PI;
            Vector3 temp_vector;

            for (float i = -pi / 2; i <= pi / 2; i += pi / 360)
            {
                for (float j = -pi; j <= pi; j += pi / 360)
                {
                    temp_vector.X = radius * (float)Math.Cos(i) * (float)Math.Cos(j) + _centerPosition.X;
                    temp_vector.Y = radius * (float)Math.Cos(i) * (float)Math.Sin(j) + _centerPosition.Y;
                    if (temp_vector.Y < _centerPosition.Y)
                        temp_vector.Y = _centerPosition.Y - height * 0.5f;
                    else
                        temp_vector.Y = _centerPosition.Y + height * 0.5f;
                    temp_vector.Z = radius * (float)Math.Sin(i) + _centerPosition.Z;
                    _vertices.Add(temp_vector);
                }
            }
        }

        public void createCylinder2(float radiusX, float radiusZ, float height, float _x, float _y, float _z)
        {
            _centerPosition.X = _x;
            _centerPosition.Y = _y;
            _centerPosition.Z = _z;
            float pi = (float)Math.PI;
            Vector3 temp_vector;

            for (float i = -pi / 2; i <= pi / 2; i += pi / 360)
            {
                for (float j = -pi; j <= pi; j += pi / 360)
                {
                    temp_vector.X = radiusX * (float)Math.Cos(i) * (float)Math.Cos(j) + _centerPosition.X;
                    temp_vector.Y = radiusZ * (float)Math.Cos(i) * (float)Math.Sin(j) + _centerPosition.Y;
                    if (temp_vector.Y < _centerPosition.Y)
                        temp_vector.Y = _centerPosition.Y - height * 0.5f;
                    else
                        temp_vector.Y = _centerPosition.Y + height * 0.5f;
                    temp_vector.Z = radiusZ * (float)Math.Sin(i) + _centerPosition.Z;
                    _vertices.Add(temp_vector);
                }
            }
        }

        public void createCylinder3(float radiusX, float radiusZ, float height, float _x, float _y, float _z,  float sectorCount, float stackCount)
        {
            _centerPosition = new Vector3(_x, _y, _z);

            float pi = (float)Math.PI;
            Vector3 temp_vector;
            float sectorStep = 2 * pi / sectorCount;
            float stackStep = pi / stackCount;
            float sectorAngle, stackAngle, tempX, tempY, tempZ;

            for (int i = 0; i <= stackCount; ++i)
            {
                stackAngle = pi / 2 - i * stackStep;

                for (int j = 0; j <= sectorCount; ++j)
                {
                    sectorAngle = j * sectorStep;

                    temp_vector.X = _x + radiusX * (float)Math.Sin(sectorAngle);
                    temp_vector.Y = _y + height * stackAngle;
                    temp_vector.Z = _z + radiusZ * (float)Math.Cos(sectorAngle);

                    _vertices.Add(temp_vector);
                }
            }

            uint k1, k2;
            for (int i = 0; i < stackCount; ++i)
            {
                k1 = (uint)(i * (sectorCount + 1));
                k2 = (uint)(k1 + sectorCount + 1);

                for (int j = 0; j < sectorCount; ++j, ++k1, ++k2)
                {
                    if (i != 0)
                    {
                        _indices.Add(k1);
                        _indices.Add(k2);
                        _indices.Add(k1 + 1);

                    }

                    if (i != stackCount - 1)
                    {
                        _indices.Add(k1 + 1);
                        _indices.Add(k2);
                        _indices.Add(k2 + 1);
                    }
                }
            }
        }

        public void createHalfCylinder(float radiusX, float radiusZ, float height, float _x, float _y, float _z)
        {
            _centerPosition.X = _x;
            _centerPosition.Y = _y;
            _centerPosition.Z = _z;
            float pi = (float)Math.PI;
            Vector3 temp_vector;

            for (float i = -pi / 2; i <= pi / 16; i += pi / 360)
            {
                for (float j = -pi; j <= pi ; j += pi / 360)
                {
                    temp_vector.X = radiusX * (float)Math.Cos(i) * (float)Math.Cos(j) + _centerPosition.X;
                    temp_vector.Y = radiusZ * (float)Math.Cos(i) * (float)Math.Sin(j) + _centerPosition.Y;
                    if (temp_vector.Y < _centerPosition.Y)
                        temp_vector.Y = _centerPosition.Y - height * 0.5f;
                    else
                        temp_vector.Y = _centerPosition.Y + height * 0.5f;
                    temp_vector.Z = radiusZ * (float)Math.Sin(i) + _centerPosition.Z;
                    _vertices.Add(temp_vector);
                }
            }
        }

        public void rotatede(Vector3 pivot, Vector3 vector, float angle)
        {
            var radangle = MathHelper.DegreesToRadians(angle);

            var arbrotationmatrix = new Matrix4
                (
                new Vector4((float)(Math.Cos(radangle) + Math.Pow(vector.X, 2.0f) * (1.0f - Math.Cos(radangle))), (float)(vector.X * vector.Y * (1.0f - Math.Cos(radangle)) + vector.Z * Math.Sin(radangle)), (float)(vector.X * vector.Z * (1.0f - Math.Cos(radangle)) - vector.Y * Math.Sin(radangle)), 0),
                new Vector4((float)(vector.X * vector.Y * (1.0f - Math.Cos(radangle)) - vector.Z * Math.Sin(radangle)), (float)(Math.Cos(radangle) + Math.Pow(vector.Y, 2.0f) * (1.0f - Math.Cos(radangle))), (float)(vector.Y * vector.Z * (1.0f - Math.Cos(radangle)) + vector.X * Math.Sin(radangle)), 0),
                new Vector4((float)(vector.X * vector.Z * (1.0f - Math.Cos(radangle)) + vector.Y * Math.Sin(radangle)), (float)(vector.Y * vector.Z * (1.0f - Math.Cos(radangle)) - vector.X * Math.Sin(radangle)), (float)(Math.Cos(radangle) + Math.Pow(vector.Z, 2.0f) * (1.0f - Math.Cos(radangle))), 0),
                Vector4.UnitW
                );

            _model *= Matrix4.CreateTranslation(-pivot);
            _model *= arbrotationmatrix;
            _model *= Matrix4.CreateTranslation(pivot);

            for (int i = 0; i < 3; i++)
            {
                _euler[i] = Vector3.Normalize(getRotationResult(pivot, vector, radangle, _euler[i], true));
            }

            _centerPosition = getRotationResult(pivot, vector, radangle, _centerPosition);

            foreach (var i in Child)
            {
                i.rotatede(pivot, vector, angle);
            }
        }

        public void rotate(Vector3 pivot, Vector3 vector, float angle)
        {
            //pivot -> mau rotate di titik mana
            //vector -> mau rotate di sumbu apa? (x,y,z)
            //angle -> rotatenya berapa derajat?
            var real_angle = angle;
            angle = MathHelper.DegreesToRadians(angle);

            //mulai ngerotasi
            for (int i = 0; i < _vertices.Count; i++)
            {
                _vertices[i] = getRotationResult(pivot, vector, angle, _vertices[i]);
            }
            //rotate the euler direction
            for (int i = 0; i < 3; i++)
            {
                _euler[i] = getRotationResult(pivot, vector, angle, _euler[i], true);

                //NORMALIZE
                //LANGKAH - LANGKAH
                //length = akar(x^2+y^2+z^2)
                float length = (float)Math.Pow(Math.Pow(_euler[i].X, 2.0f) + Math.Pow(_euler[i].Y, 2.0f) + Math.Pow(_euler[i].Z, 2.0f), 0.5f);
                Vector3 temporary = new Vector3(0, 0, 0);
                temporary.X = _euler[i].X / length;
                temporary.Y = _euler[i].Y / length;
                temporary.Z = _euler[i].Z / length;
                _euler[i] = temporary;
            }
            _centerPosition = getRotationResult(pivot, vector, angle, _centerPosition);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Count * Vector3.SizeInBytes,
                _vertices.ToArray(), BufferUsageHint.StaticDraw);
            foreach (var item in Child)
            {
                item.rotate(pivot, vector, real_angle);
            }
        }

        public Vector3 getRotationResult(Vector3 pivot, Vector3 vector, float angle, Vector3 point, bool isEuler = false)
        {
            Vector3 temp, newPosition;

            if (isEuler)
            {
                temp = point;
            }
            else
            {
                temp = point - pivot;
            }

            newPosition.X =
                temp.X * (float)(Math.Cos(angle) + Math.Pow(vector.X, 2.0f) * (1.0f - Math.Cos(angle))) +
                temp.Y * (float)(vector.X * vector.Y * (1.0f - Math.Cos(angle)) - vector.Z * Math.Sin(angle)) +
                temp.Z * (float)(vector.X * vector.Z * (1.0f - Math.Cos(angle)) + vector.Y * Math.Sin(angle));

            newPosition.Y =
                temp.X * (float)(vector.X * vector.Y * (1.0f - Math.Cos(angle)) + vector.Z * Math.Sin(angle)) +
                temp.Y * (float)(Math.Cos(angle) + Math.Pow(vector.Y, 2.0f) * (1.0f - Math.Cos(angle))) +
                temp.Z * (float)(vector.Y * vector.Z * (1.0f - Math.Cos(angle)) - vector.X * Math.Sin(angle));

            newPosition.Z =
                temp.X * (float)(vector.X * vector.Z * (1.0f - Math.Cos(angle)) - vector.Y * Math.Sin(angle)) +
                temp.Y * (float)(vector.Y * vector.Z * (1.0f - Math.Cos(angle)) + vector.X * Math.Sin(angle)) +
                temp.Z * (float)(Math.Cos(angle) + Math.Pow(vector.Z, 2.0f) * (1.0f - Math.Cos(angle)));

            if (isEuler)
            {
                temp = newPosition;
            }
            else
            {
                temp = newPosition + pivot;
            }
            return temp;
        }

        public void resetEuler()
        {
            _euler[0] = new Vector3(1, 0, 0);
            _euler[1] = new Vector3(0, 1, 0);
            _euler[2] = new Vector3(0, 0, 1);
        }
    }
}