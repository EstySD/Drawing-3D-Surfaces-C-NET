using MyMath;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RenderSpace
{
	public class Shader
	{
		Renderer.ShadingSetting shading;
		Vector cameraPos = new Vector(0, 0, 0), lightPos = new Vector(0, 0, 0);
		public Vector cameraDir = new Vector(0, 0, -1);
		int bmpWidth, bmpHeight;

		Vector triNormal;
		Color triColor;

		Vector[] vertices = new Vector[3];
		Point[] points;

		Vector[] verNormals = new Vector[3];
		Color[] verColors = new Color[3];

		float ambientStrength, diffuseStrength, specularStrength;

		Color iColor;
		Vector iNormal;
		public Shader(Renderer.ShadingSetting shading)
		{
			this.shading = shading;
		}
		public void updateCamera(Vector cameraPos, Vector cameraDir)
		{
			this.cameraPos = cameraPos;
			this.cameraDir = cameraDir;
		}
		public void updadeLight(Vector lightPos)
		{
			this.lightPos = lightPos;
		}
		public void updateLightStrength(float ambientStrength, float diffuseStrength, float specularStrength)
		{
			this.ambientStrength = ambientStrength;
			this.diffuseStrength = diffuseStrength;
			this.specularStrength = specularStrength;
		}
		public void updateClipSize(int bmpWidth, int bmpHeight)
		{
			this.bmpWidth = bmpWidth;
			this.bmpHeight = bmpHeight;
		}
		private Point convertToPixel(Vector vec)
		{
			int CenterX, CenterY;
			CenterX = bmpHeight / 2;
			CenterY = bmpWidth / 2;
			return new Point(Convert.ToInt32(vec.x * CenterX + CenterX), (int)(-vec.y * CenterY + CenterY));
		}
		public void setTri(Vector[] vertices, Vector[] verNormals, Vector triNormal)
		{
			this.vertices = vertices;
			this.verNormals = verNormals;
			this.triNormal = triNormal;


			// сортировка по y

			points = new Point[]
			{
				convertToPixel(vertices[0]),
				convertToPixel(vertices[1]),
				convertToPixel(vertices[2])
			};
			Vector tempVert;
			Point tempPoint;
			if (points[0].Y > points[1].Y)
			{
				tempVert = vertices[0];
				tempPoint = points[0];
				vertices[0] = vertices[1];
				points[0] = points[1];
				vertices[1] = tempVert;
				points[1] = tempPoint;

				tempVert = verNormals[0];
				verNormals[0] = verNormals[1];
				verNormals[1] = tempVert;
			}
			if (points[0].Y > points[2].Y)
			{
				tempVert = vertices[0];
				tempPoint = points[0];
				vertices[0] = vertices[2];
				points[0] = points[2];
				vertices[2] = tempVert;
				points[2] = tempPoint;

				tempVert = verNormals[0];
				verNormals[0] = verNormals[2];
				verNormals[2] = tempVert;
			}
			if (points[1].Y > points[2].Y)
			{
				tempVert = vertices[1];
				tempPoint = points[1];
				vertices[1] = vertices[2];
				points[1] = points[2];
				vertices[2] = tempVert;
				points[2] = tempPoint;

				tempVert = verNormals[1];
				verNormals[1] = verNormals[2];
				verNormals[2] = tempVert;
			}
		}
		public void vertexShader(Color color)
		{
			float lightStrength;
			for (int i = 0; i < 3; i++)
			{
				lightStrength = calculateLightStrength(vertices[i], verNormals[i]);
				verColors[i] = Color.FromArgb(255,
					Convert.ToInt16(BaseMath.Clamp(color.R * lightStrength, 0, 255)),
					Convert.ToInt16(BaseMath.Clamp(color.G * lightStrength, 0, 255)),
					Convert.ToInt16(BaseMath.Clamp(color.B * lightStrength, 0, 255))
				);
			}


			lightStrength = calculateLightStrength(Vector.center(vertices[0], vertices[1], vertices[2]), triNormal);
			triColor = Color.FromArgb(255,
				Convert.ToInt16(BaseMath.Clamp(color.R * lightStrength, 0, 255)),
				Convert.ToInt16(BaseMath.Clamp(color.G * lightStrength, 0, 255)),
				Convert.ToInt16(BaseMath.Clamp(color.B * lightStrength, 0, 255))
				);
		}
		void interpolate(Point p)
		{
			float x0= points[0].X; float y0= points[0].Y;
			float x1= points[1].X; float y1= points[1].Y;
			float x2= points[2].X; float y2= points[2].Y;
			float W1 = ((y1 - y2) * (p.X - x2) + (x2 - x1)*(p.Y-y2))/
				((y1 - y2) * (x0 - x2) + (x2 - x1) * (y0 - y2));
			float W2 = ((y2 - y0) * (p.X - x2) + (x0 - x2) * (p.Y - y2)) /
				((y1 - y2) * (x0 - x2) + (x2 - x1) * (y0 - y2));
			if((y1 - y2) * (x0 - x2) + (x2 - x1) * (y0 - y2) == 0)
			{
				W1 = 1;
				W2 = 0;
			}
			float W3 = 1 - W1 - W2;
			int R = Convert.ToInt16(BaseMath.Clamp(W1 * verColors[0].R + W2 * verColors[1].R + W3 * verColors[2].R,0,255));
			int G = Convert.ToInt16(BaseMath.Clamp(W1 * verColors[0].G + W2 * verColors[1].G + W3 * verColors[2].G, 0, 255));
			int B = Convert.ToInt16(BaseMath.Clamp(W1 * verColors[0].B + W2 * verColors[1].B + W3 * verColors[2].B, 0, 255));
			iColor = Color.FromArgb(255, R, G, B);
		}
		public Color fragmentShader(Point p, int lBorder, int rBorder)
		{
			switch (shading)
			{
				case Renderer.ShadingSetting.Flat:
					return triColor;
				case Renderer.ShadingSetting.Gouraud:
					interpolate(p);
					return iColor;
				case Renderer.ShadingSetting.Phong: break;
				case Renderer.ShadingSetting.FlatZ: break;
				case Renderer.ShadingSetting.Carcass:
					if (p.X == lBorder || p.X == rBorder || p.Y == points[2].Y) return triColor; 
					break;
			}
			return Color.Transparent;
		}
		float calculateLightStrength(Vector t, Vector normal)
		{
			//освещение
			float ambient = ambientStrength;

			Vector toLightDir = Vector.substract(lightPos, t).normalise();
			float diffuse = diffuseStrength * Math.Max(Vector.dotProduct(normal, toLightDir), 0);
			/*Trace.WriteLine(" diffuse: " + diffuse + " lightPos: " + lightPos.x + " " + lightPos.y + " " + lightPos.z);*/
			Vector toCameraDir = Vector.substract(t, cameraPos).normalise();


			float specular = Math.Max(2 * Vector.dotProduct(normal, toLightDir) * Vector.dotProduct(normal, toCameraDir) - Vector.dotProduct(lightPos, toCameraDir), 0);
			specular = specularStrength * (float)Math.Pow(specular, 32);


			return ambient + diffuse ;
		}
		public Point[] GetPoints()
		{
			return points;
		}
	}
}
