using OpenTK.Windowing.Desktop;
using System;
using System.Collections.Generic;
using LearnOpenTK.Common;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;


namespace Project
{
    static class Constants
    {
        public const string path = "../../../Shaders/";
    }
    internal class Window : GameWindow
    {
        List<Asset3d> _objects3d = new List<Asset3d>();
        double _time;
        Camera _camera;
        bool _firstMove = true;
        Vector2 _lastPos;
        Vector3 _objectPos = new Vector3(0, 0, 0);
        float _rotationSpeed = 0.5f;
        float wavingSpeed = 0.5f;
        float jumpSpeed = 5;

        public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
        {
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            //background
            GL.ClearColor(0.576f, 0.858f, 0.964f, 1);
            GL.Enable(EnableCap.DepthTest);

            loadBaymax();
            loadOlaf();
            loadEve();
            loadAlas();
            loadSnow(70);
            loadCloud();
            loadTree();
            loadRocks();

            foreach (Asset3d _object3d in _objects3d)
                _object3d.load(Constants.path + "shader.vert", Constants.path + "shader.frag", Size.X, Size.Y);

            initializeCamera();
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            jump(_objects3d[0]);
            waveEveHands();
            selfRotate(_objects3d[1]);
            snowFlow(new Vector3(0, 0, 0));
            cloudRotate();

            foreach (Asset3d _object3d in _objects3d)
                _object3d.render(3, _time, _camera.GetViewMatrix(), _camera.GetProjectionMatrix());

            SwapBuffers();
        }

        public void jump(Asset3d obj)
        {
            float minHeight = obj.defaultPosition.Y;
            float maxHeight = obj.defaultPosition.Y + 0.3f;

            if (obj._centerPosition.Y > maxHeight && jumpSpeed > 0)
                jumpSpeed *= -1;

            else if (obj._centerPosition.Y < minHeight && jumpSpeed < 0)
                jumpSpeed *= -1;

            obj.createTranslation(new Vector3(0, 0.001f * jumpSpeed, 0));
        }

        public void waveEveHands()
        {
            Asset3d patokan = _objects3d[2].Child[7];
            Asset3d center = _objects3d[2].Child[5];

            float minHeight = 0.36f;
            float maxHeight = 0.36f;

            if (patokan._centerPosition.Y - center._centerPosition.Y > maxHeight || center._centerPosition.Y - patokan._centerPosition.Y > minHeight)
                wavingSpeed *= -1;

            _objects3d[2].Child[6].rotatede(center._centerPosition, Vector3.UnitZ, -wavingSpeed);
            _objects3d[2].Child[7].rotatede(center._centerPosition, Vector3.UnitZ, wavingSpeed);
            patokan.rotatede(center._centerPosition, Vector3.UnitZ, wavingSpeed);
        }

        public void selfRotate(Asset3d obj)
        {
            obj.rotatede(obj._centerPosition, obj._euler[1], 2f);
        }

        public void cloudRotate()
        {
            _objects3d[5].rotatede(new Vector3(0, 0, 0), Vector3.UnitY, 0.5f);
        }

        public void snowFlow(Vector3 pivot)
        {
            _objects3d[4].rotatede(pivot, _objects3d[0]._euler[1], -1);
            _objects3d[4].rotatede(pivot, _objects3d[0]._euler[2], 1);
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, Size.X, Size.Y);
            _camera.AspectRatio = Size.X / (float)Size.Y;
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            //_camera.Fov = _camera.Fov - e.OffsetY;
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            if (KeyboardState.IsKeyDown(Keys.Escape))
                Close();

            float cameraSpeed = 1f;

            if (KeyboardState.IsKeyDown(Keys.W))
                _camera.Position += _camera.Front * cameraSpeed * (float)args.Time;

            if (KeyboardState.IsKeyDown(Keys.A))
                _camera.Position -= _camera.Right * cameraSpeed * (float)args.Time;
            
            if (KeyboardState.IsKeyDown(Keys.D))
                _camera.Position += _camera.Right * cameraSpeed * (float)args.Time;
            
            if (KeyboardState.IsKeyDown(Keys.S))
                _camera.Position -= _camera.Front * cameraSpeed * (float)args.Time;
            
            if (KeyboardState.IsKeyDown(Keys.Up))
                _camera.Position += _camera.Up * cameraSpeed * (float)args.Time;
            
            if (KeyboardState.IsKeyDown(Keys.Down))
                _camera.Position -= _camera.Up * cameraSpeed * (float)args.Time;

            if (KeyboardState.IsKeyDown(Keys.Left))
                _camera.Yaw -= cameraSpeed * (float)args.Time * 60;

            if (KeyboardState.IsKeyDown(Keys.Right))
                _camera.Yaw += cameraSpeed * (float)args.Time * 60;

            var mouse = MouseState;
            var senstivity = 0.1f;

            if (_firstMove)
            {
                _lastPos = new Vector2(mouse.X, mouse.Y);
                _firstMove = false;
            }
            else
            {
                var deltaX = mouse.X - _lastPos.X;
                var deltaY = mouse.Y - _lastPos.Y;
                _lastPos = new Vector2(mouse.X, mouse.Y);
                _camera.Yaw += deltaX * senstivity;
                _camera.Pitch -= deltaY * senstivity;
            }

            if (KeyboardState.IsKeyDown(Keys.M))
            {
                var axis = new Vector3(0, 1, 0);
                _camera.Position -= _objectPos;
                _camera.Yaw -= _rotationSpeed;
                _camera.Position = Vector3.Transform(_camera.Position, generateArbRotationMatrix(axis, _objectPos, -_rotationSpeed).ExtractRotation());
                _camera.Position += _objectPos;
                _camera._front = -Vector3.Normalize(_camera.Position - _objectPos);
            }

            if (KeyboardState.IsKeyDown(Keys.R))
            {
                initializeCamera();
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButton.Left)
            {
                float _x = (MousePosition.X - Size.X / 2) / (Size.X / 2);
                float _y = -(MousePosition.Y - Size.Y / 2) / (Size.Y / 2);

                Console.WriteLine("x = " + _x + "y = " + _y);
            }
        }

        public Matrix4 generateArbRotationMatrix(Vector3 axis, Vector3 center, float degree)
        {
            var rads = MathHelper.DegreesToRadians(degree);

            var secretFormula = new float[4, 4] {
                { (float)Math.Cos(rads) + (float)Math.Pow(axis.X, 2) * (1 - (float)Math.Cos(rads)), axis.X* axis.Y * (1 - (float)Math.Cos(rads)) - axis.Z * (float)Math.Sin(rads),    axis.X * axis.Z * (1 - (float)Math.Cos(rads)) + axis.Y * (float)Math.Sin(rads),   0 },
                { axis.Y * axis.X * (1 - (float)Math.Cos(rads)) + axis.Z * (float)Math.Sin(rads),   (float)Math.Cos(rads) + (float)Math.Pow(axis.Y, 2) * (1 - (float)Math.Cos(rads)), axis.Y * axis.Z * (1 - (float)Math.Cos(rads)) - axis.X * (float)Math.Sin(rads),   0 },
                { axis.Z * axis.X * (1 - (float)Math.Cos(rads)) - axis.Y * (float)Math.Sin(rads),   axis.Z * axis.Y * (1 - (float)Math.Cos(rads)) + axis.X * (float)Math.Sin(rads),   (float)Math.Cos(rads) + (float)Math.Pow(axis.Z, 2) * (1 - (float)Math.Cos(rads)), 0 },
                { 0, 0, 0, 1}
            };
            var secretFormulaMatix = new Matrix4
            (
                new Vector4(secretFormula[0, 0], secretFormula[0, 1], secretFormula[0, 2], secretFormula[0, 3]),
                new Vector4(secretFormula[1, 0], secretFormula[1, 1], secretFormula[1, 2], secretFormula[1, 3]),
                new Vector4(secretFormula[2, 0], secretFormula[2, 1], secretFormula[2, 2], secretFormula[2, 3]),
                new Vector4(secretFormula[3, 0], secretFormula[3, 1], secretFormula[3, 2], secretFormula[3, 3])
            );

            return secretFormulaMatix;
        }

        private void initializeCamera()
        {
            _camera = new Camera(new Vector3(0, 0.35f, 3.5f), Size.X / Size.Y);
        }

        private void loadBaymax()
        {
            //kepala
            var baymax = new Asset3d(new Vector3(0.8f, 0.8f, 0.8f));
            baymax.createEllipsoid2(0.2f, 0.1f, 0.18f, 0, 0.88f, 0, 100, 100);
            baymax.defaultPosition = new Vector3(0, 0.88f, 0);

                //mata
                var mataKiri = new Asset3d(new Vector3(0, 0, 0));
                var mataKanan = new Asset3d(new Vector3(0, 0, 0));
                mataKiri.createEllipsoid2(0.02f, 0.02f, 0.02f, -0.08f, 0.86f, 0.16f, 100, 100);
                mataKanan.createEllipsoid2(0.02f, 0.02f, 0.02f, 0.08f, 0.86f, 0.16f, 100, 100);

                //garis mata
                var garisMata = new Asset3d(new Vector3(0, 0, 0));
                garisMata.createCuboid(0.15f, 0.008f, 0.02f, 0, 0.86f, 0.17f);

            // badan
            var badanAtas = new Asset3d(new Vector3(0.9f, 0.9f, 0.9f));
            var badanBawah = new Asset3d(new Vector3(0.9f, 0.9f, 0.9f));
            badanAtas.createEllipsoid2(0.45f, 0.5f, 0.4f, 0, 0.1f, 0, 100, 100);
            badanBawah.createEllipsoid2(0.4f, 0.5f, 0.35f, 0, 0.3f, 0, 100, 100);

            //tangan
            var tanganKiri = new Asset3d(new Vector3(0.8f, 0.8f, 0.8f));
            var tanganKanan = new Asset3d(new Vector3(0.8f, 0.8f, 0.8f));
            tanganKiri.createEllipsoid2(0.14f, 0.5f, 0.13f, -0.45f, 0.28f, 0, 100, 100);
            tanganKanan.createEllipsoid2(0.14f, 0.5f, 0.13f, 0.45f, 0.28f, 0, 100, 100);
            tanganKiri.rotatede(tanganKiri._centerPosition, tanganKiri._euler[2], -20);
            tanganKanan.rotatede(tanganKanan._centerPosition, tanganKanan._euler[2], 20);

                //JariKiri
                var jariKiri1 = new Asset3d(new Vector3(0.9f, 0.9f, 0.9f));
                var jariKiri2 = new Asset3d(new Vector3(0.9f, 0.9f, 0.9f));
                var jariKiri3 = new Asset3d(new Vector3(0.9f, 0.9f, 0.9f));
                var jariKiri4 = new Asset3d(new Vector3(0.9f, 0.9f, 0.9f));
                jariKiri1.createEllipsoid2(0.03f, 0.045f, 0.04f, -0.5f, -0.15f, 0, 100, 100);
                jariKiri2.createEllipsoid2(0.03f, 0.09f, 0.01f, -0.6f, -0.2f, 0, 100, 100);
                jariKiri3.createEllipsoid2(0.03f, 0.09f, 0.01f, -0.58f, -0.2f, -0.03f, 100, 100);
                jariKiri4.createEllipsoid2(0.03f, 0.09f, 0.01f, -0.58f, -0.2f, -0.05f, 100, 100);
                jariKiri1.rotatede(jariKiri1._centerPosition, jariKiri1._euler[2], 10);
                jariKiri2.rotatede(jariKiri2._centerPosition, jariKiri2._euler[2], 30);
                jariKiri3.rotatede(jariKiri3._centerPosition, jariKiri3._euler[2], 30);
                jariKiri4.rotatede(jariKiri4._centerPosition, jariKiri4._euler[2], 30);

                //JariKanan
                var jariKanan1 = new Asset3d(new Vector3(0.9f, 0.9f, 0.9f));
                var jariKanan2 = new Asset3d(new Vector3(0.9f, 0.9f, 0.9f));
                var jariKanan3 = new Asset3d(new Vector3(0.9f, 0.9f, 0.9f));
                var jariKanan4 = new Asset3d(new Vector3(0.9f, 0.9f, 0.9f));
                jariKanan1.createEllipsoid2(0.03f, 0.045f, 0.04f, 0.5f, -0.15f, 0, 100, 100);
                jariKanan2.createEllipsoid2(0.03f, 0.09f, 0.04f, 0.6f, -0.2f, 0f, 100, 100);
                jariKanan3.createEllipsoid2(0.03f, 0.09f, 0.01f, 0.58f, -0.2f, -0.03f, 100, 100);
                jariKanan4.createEllipsoid2(0.03f, 0.09f, 0.01f, 0.58f, -0.2f, -0.05f, 100, 100);
                jariKanan1.rotatede(jariKanan1._centerPosition, jariKanan1._euler[2], -10);
                jariKanan2.rotatede(jariKanan2._centerPosition, jariKanan2._euler[2], -30);
                jariKanan3.rotatede(jariKanan3._centerPosition, jariKanan3._euler[2], -30);
                jariKanan4.rotatede(jariKanan4._centerPosition, jariKanan4._euler[2], -30);

            // kaki
            var kakiKiri = new Asset3d(new Vector3(0.8f, 0.8f, 0.8f));
            var kakiKanan = new Asset3d(new Vector3(0.8f, 0.8f, 0.8f));
            kakiKiri.createEllipsoid2(0.18f, 0.5f, 0.15f, -0.15f, -0.38f, 0, 100, 100);
            kakiKanan.createEllipsoid2(0.18f, 0.5f, 0.15f, 0.15f, -0.38f, 0f, 100, 100);

            baymax.Child.Add(mataKiri);
            baymax.Child.Add(mataKanan);
            baymax.Child.Add(garisMata);
            baymax.Child.Add(badanAtas);
            baymax.Child.Add(badanBawah);
            baymax.Child.Add(tanganKiri);
            baymax.Child.Add(tanganKanan);
            baymax.Child.Add(jariKiri1);
            baymax.Child.Add(jariKiri2);
            baymax.Child.Add(jariKiri3);
            baymax.Child.Add(jariKiri4);
            baymax.Child.Add(jariKanan1);
            baymax.Child.Add(jariKanan2);
            baymax.Child.Add(jariKanan3);
            baymax.Child.Add(jariKanan4);
            baymax.Child.Add(kakiKiri);
            baymax.Child.Add(kakiKanan);
            _objects3d.Add(baymax);
        }

        private void loadOlaf()
        {
            //kepala
            var olaf = new Asset3d(new Vector3(0.8f, 0.8f, 0.8f));
            var kepalaBawah = new Asset3d(new Vector3(0.8f, 0.8f, 0.8f));
            olaf.createEllipsoid2(0.2f, 0.35f, 0.24f, 1.9f, 0.43f, 0, 100, 100);
            kepalaBawah.createEllipsoid2(0.25f, 0.15f, 0.225f, 1.9f, 0.4f, 0, 100, 100);

                //rambut
                var rambut1 = new Asset3d(new Vector3(0.47f, 0.266f, 0.168f));
                var rambut2 = new Asset3d(new Vector3(0.47f, 0.266f, 0.168f));
                var rambut3 = new Asset3d(new Vector3(0.47f, 0.266f, 0.168f));
                var rambut4 = new Asset3d(new Vector3(0.47f, 0.266f, 0.168f));
                var rambut5 = new Asset3d(new Vector3(0.47f, 0.266f, 0.168f));
                rambut1.createCylinder(0.01f, 0.8f, 1.9f, 0.8f, 0);
                rambut2.createCylinder(0.01f, 0.35f, 1.8f, 0.75f, 0);
                rambut3.createCylinder(0.01f, 0.5f, 2f, 0.75f, 0);
                rambut4.createCylinder(0.01f, 0.1f, 2.04f, 1.033f, 0);
                rambut5.createCylinder(0.01f, 0.1f, 1.765f, 0.8f, 0);
                rambut4.rotatede(rambut4._centerPosition, rambut4._euler[2], 135);
                rambut5.rotatede(rambut5._centerPosition, rambut5._euler[2], 45);

                //dagu
                var dagu = new Asset3d(new Vector3(0.8f, 0.8f, 0.8f));
                dagu.createHalfEllipsoid(0.25f, 0.4f, 0.18f, 1.9f, 0.4f, 0, 100, 100);
                dagu.rotatede(dagu._centerPosition, dagu._euler[0], 180);

                //mulut
                var mulut = new Asset3d(new Vector3(0.2f, 0.2f, 0.2f));
                mulut.createHalfEllipsoid(0.18f, 0.27f, 0.09f, 1.9f, 0.4f, 0.15f, 100, 100);
                mulut.rotatede(mulut._centerPosition, mulut._euler[0], 185);

                    //gigi
                    var gigi = new Asset3d(new Vector3(1, 1, 1));
                    gigi.createCuboid(0.12f, 0.07f, 0.01f, 1.9f, 0.358f, 0.235f);
                    gigi.rotatede(gigi._centerPosition, gigi._euler[0], 7);

                //hidung
                var hidung = new Asset3d(new Vector3(0.96f, 0.345f, 0.019f));
                hidung.createEllipsoid2(0.03f, 0.03f, 0.2f, 1.9f, 0.47f, 0.3f, 100, 100);

                //mata
                //dalam
                var mataDalamKiri = new Asset3d(new Vector3(0.95f, 0.95f, 0.95f));
                var mataDalamKanan = new Asset3d(new Vector3(0.95f, 0.95f, 0.95f));
                mataDalamKiri.createCylinder(0.06f, 0.01f, 1.82f, 0.55f, 0.2f);
                mataDalamKanan.createCylinder(0.06f, 0.01f, 1.98f, 0.55f, 0.2f);
                mataDalamKiri.rotatede(mataDalamKiri._centerPosition, mataDalamKiri._euler[0], -100);
                mataDalamKanan.rotatede(mataDalamKanan._centerPosition, mataDalamKanan._euler[0], -100);
                mataDalamKiri.rotatede(mataDalamKiri._centerPosition, mataDalamKiri._euler[2], 150);
                mataDalamKanan.rotatede(mataDalamKanan._centerPosition, mataDalamKanan._euler[2], -150);

                    //luar
                    var mataLuarKiri = new Asset3d(new Vector3(0, 0, 0));
                    var mataLuarKanan = new Asset3d(new Vector3(0, 0, 0));
                    mataLuarKiri.createCylinder(0.025f, 0.01f, 1.82f, 0.53f, 0.21f);
                    mataLuarKanan.createCylinder(0.025f, 0.01f, 1.98f, 0.53f, 0.21f);
                    mataLuarKiri.rotatede(mataLuarKiri._centerPosition, mataLuarKiri._euler[0], -100);
                    mataLuarKanan.rotatede(mataLuarKanan._centerPosition, mataLuarKanan._euler[0], -100);
                    mataLuarKiri.rotatede(mataLuarKiri._centerPosition, mataLuarKiri._euler[2], 150);
                    mataLuarKanan.rotatede(mataLuarKanan._centerPosition, mataLuarKanan._euler[2], -150);

                //alis
                var alisKiri = new Asset3d(new Vector3(0.1f, 0.1f, 0.1f));
                var alisKanan = new Asset3d(new Vector3(0.1f, 0.1f, 0.1f));
                alisKiri.createHalfCylinder(0.06f, 0.03f, 0.008f, 1.835f, 0.67f, 0.14f);
                alisKanan.createHalfCylinder(0.06f, 0.03f, 0.008f, 1.965f, 0.67f, 0.14f);
                alisKiri.rotatede(alisKiri._centerPosition, alisKiri._euler[0], 90);
                alisKanan.rotatede(alisKanan._centerPosition, alisKanan._euler[0], 90);
                alisKiri.rotatede(alisKiri._centerPosition, alisKiri._euler[2], -165);
                alisKanan.rotatede(alisKanan._centerPosition, alisKanan._euler[2], 165);
                alisKiri.rotatede(alisKiri._centerPosition, alisKiri._euler[1], -20);
                alisKanan.rotatede(alisKanan._centerPosition, alisKanan._euler[1], 20);

                

            //badan
            var badanAtas = new Asset3d(new Vector3(0.9f, 0.9f, 0.9f));
            var badanBawah = new Asset3d(new Vector3(0.9f, 0.9f, 0.9f));
            badanAtas.createEllipsoid2(0.23f, 0.17f, 0.18f, 1.9f, -0.03f, 0, 100, 100); 
            badanBawah.createEllipsoid2(0.37f, 0.35f, 0.27f, 1.9f, -0.37f, 0, 100, 100); 

                    //kancing
                    var kancing1 = new Asset3d(new Vector3(0.2f, 0.2f, 0.2f));
                    var kancing2 = new Asset3d(new Vector3(0.2f, 0.2f, 0.2f));
                    var kancing3 = new Asset3d(new Vector3(0.2f, 0.2f, 0.2f));
                    kancing1.createEllipsoid2(0.06f, 0.06f, 0.04f, 1.9f, 0f, 0.15f, 100, 100);
                    kancing2.createEllipsoid2(0.06f, 0.06f, 0.04f, 1.9f, -0.25f, 0.24f, 100, 100);
                    kancing3.createEllipsoid2(0.06f, 0.06f, 0.04f, 1.9f, -0.45f, 0.24f, 100, 100);

                //tangan

                //kiri
                var tanganKiri = new Asset3d(new Vector3(0.47f, 0.266f, 0.168f));
                tanganKiri.createCylinder(0.01f, 0.6f, 1.5f, 0.18f, 0);
                tanganKiri.rotatede(tanganKiri._centerPosition, tanganKiri._euler[2], 50);
                    //jari
                    var jariKiri1 = new Asset3d(new Vector3(0.47f, 0.266f, 0.168f));
                    var jariKiri2 = new Asset3d(new Vector3(0.47f, 0.266f, 0.168f));
                    var jariKiri3 = new Asset3d(new Vector3(0.47f, 0.266f, 0.168f));
                    var jariKiri4 = new Asset3d(new Vector3(0.47f, 0.266f, 0.168f));
                    jariKiri1.createCylinder(0.01f, 0.15f, 1.23f, 0.4f, 0);
                    jariKiri2.createCylinder(0.01f, 0.1f, 1.33f, 0.38f, 0);
                    jariKiri3.createCylinder(0.01f, 0.1f, 1.26f,0.34f, 0);
                    jariKiri4.createCylinder(0.01f, 0.16f, 1.29f, 0.41f, 0);
                    jariKiri1.rotatede(jariKiri1._centerPosition, jariKiri1._euler[2], 50);
                    jariKiri2.rotatede(jariKiri2._centerPosition, jariKiri2._euler[2], 145);
                    jariKiri3.rotatede(jariKiri3._centerPosition, jariKiri3._euler[2], 90);
                    jariKiri4.rotatede(jariKiri4._centerPosition, jariKiri4._euler[2], 190);

                //kanan
                var tanganKanan = new Asset3d(new Vector3(0.47f, 0.266f, 0.168f));
                tanganKanan.createCylinder(0.01f, 0.6f, 2.3f, -0.05f, 0);
                tanganKanan.rotatede(tanganKanan._centerPosition, tanganKanan._euler[2], 80);

                    //jari
                    var jariKanan1 = new Asset3d(new Vector3(0.47f, 0.266f, 0.168f));
                    var jariKanan2 = new Asset3d(new Vector3(0.47f, 0.266f, 0.168f));
                    var jariKanan3 = new Asset3d(new Vector3(0.47f, 0.266f, 0.168f));
                    var jariKanan4 = new Asset3d(new Vector3(0.47f, 0.266f, 0.168f));
                    jariKanan1.createCylinder(0.01f, 0.07f, 2.55f, -0.13f, 0);
                    jariKanan2.createCylinder(0.01f, 0.1f, 2.65f, -0.11f, 0);
                    jariKanan3.createCylinder(0.01f, 0.12f, 2.6f, -0.13f, 0);
                    jariKanan4.createCylinder(0.01f, 0.1f, 2.6f, -0.063f, 0);
                    jariKanan1.rotatede(jariKanan1._centerPosition, jariKanan1._euler[2], 30);
                    jariKanan2.rotatede(jariKanan2._centerPosition, jariKanan2._euler[2], 80);
                    jariKanan3.rotatede(jariKanan3._centerPosition, jariKanan3._euler[2], 60);
                    jariKanan4.rotatede(jariKanan4._centerPosition, jariKanan4._euler[2], 120);

            //kaki
            var kakiKiri = new Asset3d(new Vector3(0.8f, 0.8f, 0.8f));
            var kakiKanan = new Asset3d(new Vector3(0.8f, 0.8f, 0.8f));
            kakiKiri.createEllipsoid2(0.11f, 0.13f, 0.11f, 1.75f, -0.75f, 0, 100, 100); //kiri
            kakiKanan.createEllipsoid2(0.11f, 0.13f, 0.11f, 2.05f, -0.75f, 0, 100, 100); //kanan

            olaf.Child.Add(kepalaBawah);
            olaf.Child.Add(rambut1);
            olaf.Child.Add(rambut2);
            olaf.Child.Add(rambut3);
            olaf.Child.Add(rambut4);
            olaf.Child.Add(rambut5);
            olaf.Child.Add(dagu);
            olaf.Child.Add(mulut);
            olaf.Child.Add(gigi);
            olaf.Child.Add(hidung);
            olaf.Child.Add(alisKiri);
            olaf.Child.Add(alisKanan);
            olaf.Child.Add(mataDalamKiri);
            olaf.Child.Add(mataDalamKanan);
            olaf.Child.Add(mataLuarKiri);
            olaf.Child.Add(mataLuarKanan);
            olaf.Child.Add(badanAtas);
            olaf.Child.Add(badanBawah);
            olaf.Child.Add(kancing1);
            olaf.Child.Add(kancing2);
            olaf.Child.Add(kancing3);
            olaf.Child.Add(tanganKiri); //22
            olaf.Child.Add(tanganKanan);
            olaf.Child.Add(jariKiri1); //.
            olaf.Child.Add(jariKiri2);
            olaf.Child.Add(jariKiri3);
            olaf.Child.Add(jariKiri4);
            olaf.Child.Add(jariKanan1);
            olaf.Child.Add(jariKanan2); //.
            olaf.Child.Add(jariKanan3);
            olaf.Child.Add(jariKanan4);
            olaf.Child.Add(kakiKiri);
            olaf.Child.Add(kakiKanan);
            olaf.createTranslation(new Vector3(0, 0, 0.1f));
            _objects3d.Add(olaf);
        }

        private void loadEve()
        {
            //kepala
            var eve =  new Asset3d(new Vector3(0.9f, 0.9f, 0.9f));
            var kepalaBawah = new Asset3d(new Vector3(0.9f, 0.9f, 0.9f));
            eve.createHalfEllipsoid(0.25f, 0.28f, 0.2f, -1.9f, 0.3f, 0.0f, 100, 100);
            kepalaBawah.createEllipsoid2(0.25f, 0.13f, 0.2f, -1.9f, 0.3f, 0.0f, 100, 100);

                //muka
                var mukaAtas = new Asset3d(new Vector3(0.05f, 0.05f, 0.05f));
                var mukaBawah = new Asset3d(new Vector3(0.05f, 0.05f, 0.05f));
                mukaAtas.createHalfEllipsoid(0.185f, 0.18f, 0.1f, -1.88f, 0.31f, 0.12f, 100, 100);
                mukaBawah.createEllipsoid2(0.185f, 0.09f, 0.1f, -1.88f, 0.31f, 0.12f, 100, 100);

                    //mata
                    var mataKiri = new Asset3d(new Vector3(0.105f, 0.45f, 0.972f));
                    var mataKanan = new Asset3d(new Vector3(0.105f, 0.45f, 0.972f));
                    mataKiri.createCylinder2(0.05f, 0.03f, 0.01f, -1.93f, 0.355f, 0.21f);
                    mataKanan.createCylinder2(0.05f, 0.03f, 0.01f, -1.79f, 0.35f, 0.2f);
                    mataKiri.rotatede(mataKiri._centerPosition, mataKiri._euler[0], 82);
                    mataKanan.rotatede(mataKanan._centerPosition, mataKanan._euler[0], 82);
                    mataKiri.rotatede(mataKiri._centerPosition, mataKiri._euler[2], 8);
                    mataKanan.rotatede(mataKanan._centerPosition, mataKanan._euler[2], -13);
                    mataKiri.rotatede(mataKiri._centerPosition, mataKiri._euler[1], -16);
                    mataKanan.rotatede(mataKanan._centerPosition, mataKanan._euler[1], 14);

            //badan
            var badan = new Asset3d(new Vector3(0.9f, 0.9f, 0.9f));
            badan.createHalfEllipsoid(0.25f, 0.75f, 0.2f, -1.9f, 0.05f, 0, 100, 100);
            badan.rotatede(badan._centerPosition, badan._euler[0], 180);

            //tangan
            var tanganKiri = new Asset3d(new Vector3(0.8f, 0.8f, 0.8f));
            var tanganKanan = new Asset3d(new Vector3(0.8f, 0.8f, 0.8f));
            tanganKiri.createEllipsoid2(0.08f, 0.375f, 0.11f, -2.6f, -0.25f, 0, 100, 100);
            tanganKanan.createEllipsoid2(0.08f, 0.375f, 0.11f, -1.3f, -0.3f, 0, 100, 100);
            tanganKiri.rotatede(tanganKiri._centerPosition, tanganKiri._euler[2], -50);
            tanganKanan.rotatede(tanganKanan._centerPosition, tanganKanan._euler[2], 20);

            eve.Child.Add(kepalaBawah);
            eve.Child.Add(mukaAtas);
            eve.Child.Add(mukaBawah);
            eve.Child.Add(mataKiri);
            eve.Child.Add(mataKanan);
            eve.Child.Add(badan);
            eve.Child.Add(tanganKiri);
            eve.Child.Add(tanganKanan);
            _objects3d.Add(eve);
        }

        private void loadAlas()
        {
            //atas
            var alas = new Asset3d(new Vector3(1, 1, 1));
            var alasAtas2 = new Asset3d(new Vector3(1, 1, 1));
            alas.createCylinder3(2.3f, 1.9f, 0.03f, 0, -0.85f, 0.0f, 100, 100);
            alasAtas2.createEllipsoid2(2.3f, 0, 1.9f, 0, -0.85f, 0.0f, 100, 100);

            //bawah
            var alasBawah = new Asset3d(new Vector3(0.47f, 0.266f, 0.168f));
            alasBawah.createHalfEllipsoid(2.5f, 0.5f, 2.1f, 0, -0.8f, 0.0f, 100, 100);
            alasBawah.rotatede(alasBawah._centerPosition, alasBawah._euler[0], 180);

            alas.Child.Add(alasAtas2);
            alas.Child.Add(alasBawah);
            _objects3d.Add(alas);
        }

        private void loadSnow(int snowCount)
        {
            var snow = new Asset3d(new Vector3(1, 1, 1));

            for (int i = 0; i < snowCount; i++)
            {
                float x = NextFloat(-2.5f, 2.5f);
                float y = NextFloat(-0.5f, 2);
                float z = NextFloat(-1.8f, 1.8f);
                float sizeX = NextFloat(0, 0.03f);
                float sizeY = NextFloat(0, 0.03f);
                float sizeZ = NextFloat(0, 0.03f);

                snow.Child.Add(new Asset3d(new Vector3(1, 1, 1)));
                snow.Child[i].createEllipsoid2(sizeX, sizeY, sizeZ, x, y, z, 100, 100);
            }

            _objects3d.Add(snow);
        }

        private float NextFloat(float min, float max)
        {
            System.Random random = new System.Random();
            double val = (random.NextDouble() * (max - min) + min);
            return (float)val;
        }

        private void loadCloud()
        {
            var cloud = new Asset3d(new Vector3(1, 1, 1));
            var cloud2 = new Asset3d(new Vector3(1, 1, 1));
            var cloud3 = new Asset3d(new Vector3(1, 1, 1));
            cloud.createEllipsoid2(1.2f, 0.8f, 1, 0, 3f, -5.5f, 100, 100);
            cloud2.createEllipsoid2(1.5f, 0.5f, 0.8f, -1.5f, 3, -5.5f, 100, 100);
            cloud3.createEllipsoid2(1.3f, 0.6f, 0.7f, 1.5f, 3, -5.5f, 100, 100);

            cloud.Child.Add(cloud2);
            cloud.Child.Add(cloud3);
            _objects3d.Add(cloud);
        }

        private void loadTree()
        {
            var trees = new Asset3d();

            var tree1 = new Asset3d(new Vector3(0.701f, 0.349f, 0.258f));
            var daun1Tree1 = new Asset3d(new Vector3(0.803f, 0.933f, 0.749f));
            var daun2Tree1 = new Asset3d(new Vector3(0.501f, 0.862f, 0.337f));
            var daun3Tree1 = new Asset3d(new Vector3(0.341f, 0.556f, 0.243f));

            tree1.createCylinder3(0.1f, 0.1f, 1, 0, 0.5f, -1.6f, 100, 100);
            daun1Tree1.createHalfEllipsoid(0.4f, 3.5f, 0.43f, 0, 0.3f, -1.6f, 100, 100);
            daun2Tree1.createHalfEllipsoid(0.7f, 2f, 0.73f, 0, 0.5f, -1.6f, 100, 100);
            daun3Tree1.createHalfEllipsoid(1, 1.5f, 1, 0, -0.2f, -1.6f, 100, 100);

            var tree2 = new Asset3d(new Vector3(0.701f, 0.349f, 0.258f));
            var daun1Tree2 = new Asset3d(new Vector3(0.803f, 0.933f, 0.749f));
            var daun2Tree2 = new Asset3d(new Vector3(0.501f, 0.862f, 0.337f));
            var daun3Tree2 = new Asset3d(new Vector3(0.341f, 0.556f, 0.243f));

            tree2.createCylinder3(0.1f, 0.1f, 1, 0, 0.5f, -1.6f, 100, 100);
            daun1Tree2.createHalfEllipsoid(0.4f, 3.5f, 0.43f, 0, 0.3f, -1.6f, 100, 100);
            daun2Tree2.createHalfEllipsoid(0.7f, 2f, 0.73f, 0, 0.5f, -1.6f, 100, 100);
            daun3Tree2.createHalfEllipsoid(1, 1.5f, 1, 0, -0.2f, -1.6f, 100, 100);

            var tree3 = new Asset3d(new Vector3(0.701f, 0.349f, 0.258f));
            var daun1Tree3 = new Asset3d(new Vector3(0.803f, 0.933f, 0.749f));
            var daun2Tree3 = new Asset3d(new Vector3(0.501f, 0.862f, 0.337f));
            var daun3Tree3 = new Asset3d(new Vector3(0.341f, 0.556f, 0.243f));

            tree3.createCylinder3(0.1f, 0.1f, 1, 0, 0.5f, -1.6f, 100, 100);
            daun1Tree3.createHalfEllipsoid(0.4f, 3.5f, 0.43f, 0, 0.3f, -1.6f, 100, 100);
            daun2Tree3.createHalfEllipsoid(0.7f, 2f, 0.73f, 0, 0.5f, -1.6f, 100, 100);
            daun3Tree3.createHalfEllipsoid(1, 1.5f, 1, 0, -0.2f, -1.6f, 100, 100);

            tree1.Child.Add(daun1Tree1);
            tree1.Child.Add(daun2Tree1);
            tree1.Child.Add(daun3Tree1);

            tree2.Child.Add(daun1Tree2);
            tree2.Child.Add(daun2Tree2);
            tree2.Child.Add(daun3Tree2);

            tree3.Child.Add(daun1Tree3);
            tree3.Child.Add(daun2Tree3);
            tree3.Child.Add(daun3Tree3);

            tree1.createTranslation(new Vector3(-1.3f,0,0));
            tree1.createTranslation(new Vector3(0, 0, -0.2f));
            tree3.createTranslation(new Vector3(1.3f, 0, 0));

            trees.Child.Add(tree1);
            trees.Child.Add(tree2);
            trees.Child.Add(tree3);

            _objects3d.Add(trees);
        }

        private void loadRocks()
        {
            var rocks = new Asset3d(new Vector3(0.6f, 0.6f, 0.6f));
            var rock1 = new Asset3d(new Vector3(0.6f, 0.6f, 0.6f));
            var rock2 = new Asset3d(new Vector3(0.6f, 0.6f, 0.6f));
            var rock3 = new Asset3d(new Vector3(0.6f, 0.6f, 0.6f));
            var rock4 = new Asset3d(new Vector3(0.6f, 0.6f, 0.6f));
            var rock5 = new Asset3d(new Vector3(0.6f, 0.6f, 0.6f));
            var rock6 = new Asset3d(new Vector3(0.6f, 0.6f, 0.6f));
            var rock7 = new Asset3d(new Vector3(0.6f, 0.6f, 0.6f));
            var rock8 = new Asset3d(new Vector3(0.6f, 0.6f, 0.6f));
            var rock9 = new Asset3d(new Vector3(0.6f, 0.6f, 0.6f));
            var rock10 = new Asset3d(new Vector3(0.6f, 0.6f, 0.6f));
            var rock11 = new Asset3d(new Vector3(0.6f, 0.6f, 0.6f));
            var rock12 = new Asset3d(new Vector3(0.6f, 0.6f, 0.6f));
            var rock13 = new Asset3d(new Vector3(0.6f, 0.6f, 0.6f));
            var rock14 = new Asset3d(new Vector3(0.6f, 0.6f, 0.6f));
            var rock15 = new Asset3d(new Vector3(0.6f, 0.6f, 0.6f));
            var rock16 = new Asset3d(new Vector3(0.6f, 0.6f, 0.6f));
            var rock17 = new Asset3d(new Vector3(0.6f, 0.6f, 0.6f));
            var rock18 = new Asset3d(new Vector3(0.6f, 0.6f, 0.6f));
            var rock19 = new Asset3d(new Vector3(0.6f, 0.6f, 0.6f));
            var rock20 = new Asset3d(new Vector3(0.6f, 0.6f, 0.6f));
            var rock21 = new Asset3d(new Vector3(0.6f, 0.6f, 0.6f));
            var rock22 = new Asset3d(new Vector3(0.6f, 0.6f, 0.6f));

            rocks.createEllipsoid2(0.2f, 0.15f, 0.18f, 0.4f, -0.7f, 1.78f, 100, 100);
            rock1.createEllipsoid2(0.2f, 0.15f, 0.18f, 0, -0.7f, 1.8f, 100, 100);
            rock2.createEllipsoid2(0.2f, 0.15f, 0.18f, -0.4f, -0.7f, 1.8f, 100, 100);
            rock3.createEllipsoid2(0.2f, 0.15f, 0.18f, -0.77f, -0.7f, 1.7f, 100, 100);
            rock4.createEllipsoid2(0.2f, 0.15f, 0.18f, -1.12f, -0.7f, 1.55f, 100, 100);
            rock5.createEllipsoid2(0.2f, 0.15f, 0.18f, -1.44f, -0.7f, 1.35f, 100, 100);
            rock6.createEllipsoid2(0.2f, 0.15f, 0.18f, -1.67f, -0.7f, 1.1f, 100, 100);
            rock7.createEllipsoid2(0.2f, 0.15f, 0.18f, 0.8f, -0.7f, 1.7f, 100, 100);
            rock8.createEllipsoid2(0.2f, 0.15f, 0.18f, 1.4f, -0.7f, 1.33f, 100, 100);
            rock9.createEllipsoid2(0.2f, 0.15f, 0.18f, 1.1f, -0.7f, 1.55f, 100, 100);
            rock10.createEllipsoid2(0.2f, 0.15f, 0.18f, -1.87f, -0.7f, 0.8f, 100, 100);
            rock11.createEllipsoid2(0.2f, 0.15f, 0.18f, 1.65f, -0.7f, 1.1f, 100, 100);
            rock12.createEllipsoid2(0.2f, 0.15f, 0.18f, -0.03f, -0.7f, 1.3f, 100, 100);
            rock13.createEllipsoid2(0.2f, 0.15f, 0.18f, -0.03f, -0.7f, 0.9f, 100, 100);
            rock14.createEllipsoid2(0.2f, 0.15f, 0.18f, -0.03f, -0.7f, 0.4f, 100, 100);
            rock15.createEllipsoid2(0.2f, 0.15f, 0.18f, 0.35f, -0.7f, 0.45f, 100, 100);
            rock16.createEllipsoid2(0.2f, 0.15f, 0.18f, 0.7f, -0.75f, 0.48f, 100, 100);
            rock17.createEllipsoid2(0.2f, 0.15f, 0.18f, 1.07f, -0.75f, 0.65f, 100, 100);
            rock18.createEllipsoid2(0.2f, 0.15f, 0.18f, 1.45f, -0.75f, 0.82f, 100, 100);
            rock19.createEllipsoid2(0.2f, 0.15f, 0.18f, -0.39f, -0.7f, 0.47f, 100, 100);
            rock20.createEllipsoid2(0.2f, 0.15f, 0.18f, -0.75f, -0.7f, 0.54f, 100, 100);
            rock21.createEllipsoid2(0.2f, 0.15f, 0.18f, -1.1f, -0.7f, 0.58f, 100, 100);
            rock22.createEllipsoid2(0.2f, 0.15f, 0.18f, -1.5f, -0.7f, 0.62f, 100, 100);

            rock2.rotatede(rock2._centerPosition, rock2._euler[0], 180);
            rock2.rotatede(rock2._centerPosition, rock2._euler[0], 180);
            rock2.rotatede(rock2._centerPosition, rock2._euler[0], 180);
            rock2.rotatede(rock2._centerPosition, rock2._euler[0], 180);
            rock3.rotatede(rock3._centerPosition, rock3._euler[0], 170);
            rock4.rotatede(rock4._centerPosition, rock4._euler[0], 175);
            rock5.rotatede(rock5._centerPosition, rock5._euler[0], 175);
            rock6.rotatede(rock6._centerPosition, rock6._euler[0], 175);

            rocks.Child.Add(rock1);
            rocks.Child.Add(rock2);
            rocks.Child.Add(rock3);
            rocks.Child.Add(rock4);
            rocks.Child.Add(rock5);
            rocks.Child.Add(rock6);
            rocks.Child.Add(rock7);
            rocks.Child.Add(rock8);
            rocks.Child.Add(rock9);
            rocks.Child.Add(rock10);
            rocks.Child.Add(rock11);
            rocks.Child.Add(rock12);
            rocks.Child.Add(rock13);
            rocks.Child.Add(rock14);
            rocks.Child.Add(rock15);
            rocks.Child.Add(rock16);
            rocks.Child.Add(rock17);
            rocks.Child.Add(rock18);
            rocks.Child.Add(rock19);
            rocks.Child.Add(rock20);
            rocks.Child.Add(rock21);
            rocks.Child.Add(rock22);

            _objects3d.Add(rocks);
        }
    }
}
