using System;
using System.Drawing;
using System.Windows.Forms;

using MyMath;
using RenderSpace;
using Mesh;
using System.Threading.Tasks;
using System.Diagnostics;

namespace visualisation_lr1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        Renderer.ShadingSetting shadingSetting = Renderer.ShadingSetting.Flat;
        bool fColorChoosed = true;

        Color fColor;
        Color bColor = Color.Red;

        TrackHandler Xc, Yc, Zc, N1c, N2c, R1c, R2c, uMaxc, vMaxc, Rc, Gc, Bc;
        TrackHandler XL, YL, ZL;
        int xOld, yOld, zOld;

		MeshInfo meshInfo = new MeshInfo();/*
		float[,] transformMatrix = Matrix.multiplyMatrix(Matrix.getScale(0.75f, 0.75f, 0.75f), Matrix.getUnit());*/
        float[,] transformMatrix = Matrix.getUnit();


		surface surf = new surface(0, 360, -90, 90);

        private void Form1_Load(object sender, EventArgs e)
        {
            //применение обновления кнопок и трекбаров
            Xc = new TrackHandler(this, "X", -180, 180, 0);
            Yc = new TrackHandler(this, "Y", -180, 180, 0);
            Zc = new TrackHandler(this, "Z", -180, 180, 0);
            N1c = new TrackHandler(this, "N1", 4, 100, 4);
            N2c = new TrackHandler(this, "N2", 4, 100, 4);
            R1c = new TrackHandler(this, "R1", 10, 100, 100);
            R2c = new TrackHandler(this, "R2", 10, 100, 100);
            uMaxc = new TrackHandler(this, "UMax", surf.umin, surf.umax, surf.umax);
            vMaxc = new TrackHandler(this, "VMax", surf.vmin, surf.vmax, surf.vmax);
            Rc = new TrackHandler(this, "R", 0, 255, 0);
            Gc = new TrackHandler(this, "G", 0, 255, 255);
            Bc = new TrackHandler(this, "B", 0, 255, 0);
			XL = new TrackHandler(this, "XL", -100, 100, 0);
			YL = new TrackHandler(this, "YL", -100, 100, 0);
			ZL = new TrackHandler(this, "ZL", -100, 100, 0);
			foreach (Control c in this.groupBoxFill.Controls)
            {
                RadioButton rb = c as RadioButton;
                if (rb!=null) rb.CheckedChanged += rbUpdate;
            }

            canvas_update(null, null);
		}

        public void canvas_update(object sender, EventArgs e)
        {
            //обновление значений

            Color color = Color.FromArgb(255, Rc.value, Gc.value, Bc.value);
            if (fColorChoosed)
				frontColorButton.BackColor = fColor = color;
            else backColorButton.BackColor = bColor = color;

			//
            meshInfo.setInterval(BaseMath.ConvertToRad(surf.umin), BaseMath.ConvertToRad(uMaxc.value),
				BaseMath.ConvertToRad(surf.vmin), BaseMath.ConvertToRad(vMaxc.value));
            meshInfo.setSettings(surf, N1c.value, N2c.value, (float)(R1c.value) / 100, (float)(R2c.value) / 100);
            meshInfo.calculate();

			Vector[] vertices = meshInfo.getVertices();
            int[,] indices = meshInfo.getIndices();
			// matrix transf
			transformMatrix = Matrix.multiplyMatrix(Matrix.getRotation(Xc.value - xOld, Matrix.Axis.X), transformMatrix);
            transformMatrix = Matrix.multiplyMatrix(Matrix.getRotation(Yc.value - yOld, Matrix.Axis.Y), transformMatrix);
            transformMatrix = Matrix.multiplyMatrix(Matrix.getRotation(Zc.value - zOld, Matrix.Axis.Z), transformMatrix);
			xOld = Xc.value; yOld = Yc.value; zOld = Zc.value;
			/*transformMatrix = Matrix.multiplyMatrix(Matrix.getViewMatrix(new Vector(0, 0, 3), new Vector(0, 0, 0), new Vector(0, 1, 0)), transformMatrix);*/

			Vector[] transVertices = new Vector[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                transVertices[i] = Matrix.multiplyVector(transformMatrix, vertices[i]);
            }

            //визуализация

            Renderer render = new Renderer(pictureBox1.Width, pictureBox1.Height);
			render.updateData(transVertices, indices);
			render.updateColor(fColor, bColor);

			Shader shader = new Shader(shadingSetting);
			shader.updadeLight(new Vector((float)XL.value/100, (float)YL.value / 100, 2.5f));
			shader.updateLightStrength(0.1f, 1f, 1f);
			shader.updateCamera(new Vector(0, 0, 0), new Vector(0, 0, -1));

			render.renderPass(Renderer.CullSetting.FrontFace, shader);
			render.renderPass(Renderer.CullSetting.BackFace, shader);
			/* render.shader = shader;
             shader.updateClipSize(pictureBox1.Width, pictureBox1.Height);
             render.DrawTri(Color.Green, 
                 new Vector[3]
                 {
                     new Vector(0, 0.5f, 0),
                     new Vector(0, 0, 0),
                     new Vector(0.5f,-0.25f,0)
                 },
                 new Vector(0,0,-1),
                 new Vector[3]
                 {
                     new Vector(0,0,-1),
                     new Vector(0,0,-1),
                     new Vector(0,0,-1)
                 }
                 );*/
			/*render.DrawAxis(0.75f, 2);*/
			//вывод
			pictureBox1.Refresh();
            pictureBox1.Image = render.getImage();
            this.Update();
        }

       
        

        //update buttons
		private void frontColorButton_Click(object sender, EventArgs e)
		{
			fColorChoosed = true;
			trackBarR.Value = Rc.value = fColor.R;
			trackBarG.Value = Gc.value = fColor.G;
			trackBarB.Value = Bc.value = fColor.B;
		}

		private void backColorButton_Click(object sender, EventArgs e)
		{
			fColorChoosed = false;
			trackBarR.Value = Rc.value = bColor.R;
			trackBarG.Value = Gc.value = bColor.G;
			trackBarB.Value = Bc.value = bColor.B;
		}

        private void rbUpdate(object sender, EventArgs e)
        {
			RadioButton btn = sender as RadioButton;
			if (btn != null && btn.Checked)
            {
                switch (btn.Name) {
                    case "radioButtonCarcass":
                        shadingSetting = Renderer.ShadingSetting.Carcass;
                        break;
                    case "radioButtonFlat":
						shadingSetting = Renderer.ShadingSetting.Flat;
						break;
                    case "radioButtonFlatZ":
						shadingSetting = Renderer.ShadingSetting.FlatZ;
						break;
                    case "radioButtonGouraud":
						shadingSetting = Renderer.ShadingSetting.Gouraud;
						break;
					case "radioButtonPhong":
						shadingSetting = Renderer.ShadingSetting.Phong;
						break;

				}
            };
            canvas_update(null, null);
		}
		class TrackHandler
        {
            public int value { get; set; }
            System.Windows.Forms.TrackBar tBar;
            System.Windows.Forms.NumericUpDown numUD;
            public TrackHandler(Form1 form1, string name, int min, int max, int defValue)
            {
                tBar = (System.Windows.Forms.TrackBar)form1.Controls.Find("trackBar" + name, true)[0];
                numUD = (System.Windows.Forms.NumericUpDown)form1.Controls.Find("numericUpDown" + name, true)[0];
				tBar.Minimum = min;
				tBar.Maximum = max;
				numUD.Minimum = min;
				numUD.Maximum = max;
                tBar.Value = defValue;
                numUD.Value = defValue;

				tBar.Scroll += tBarUpdate;
                numUD.ValueChanged += numUDUpdate;
                numUD.ValueChanged += form1.canvas_update;

                System.Windows.Forms.Label minL = (System.Windows.Forms.Label)form1.Controls.Find("l_" + name + "_Min", true)[0];
                System.Windows.Forms.Label maxL = (System.Windows.Forms.Label)form1.Controls.Find("l_" + name + "_Max", true)[0];
                minL.Text = Convert.ToString(min);
                maxL.Text = Convert.ToString(max);
                tBarUpdate(null, null);
                numUDUpdate(null, null);
            }
            void tBarUpdate(object sender, EventArgs e)
            {
                this.value = tBar.Value;
                numUD.Value = value;
            }
            void numUDUpdate(object sender, EventArgs e)
            {
                this.value = (int)numUD.Value;
                tBar.Value =value;

            }
        }

    }
}
