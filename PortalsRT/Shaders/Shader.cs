using System;
using System.Collections.Generic;
using System.IO;

using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace PortalsRT.Shaders
{
    public class Shader
    {

        public ShaderType Type { get; private set; } = ShaderType.FragmentShader;

        public int ID { get; private set; }

        public string ShaderSource { get; private set; } = "";

        public Shader(ShaderType shaderType, string file = null)
        {
            Type = shaderType;

            ID = GL.CreateShader(Type);

            if (file != null)
            {
                SetShaderFile(file);
            }
        }

        /// <summary>
        /// Load shader from source code.
        /// </summary>
        /// <param name="shaderSource">Shader source</param>
        /// <returns>self</returns>
        public Shader SetShaderSource(string shaderSource)
        {
            ShaderSource = shaderSource;

            if (shaderSource.Trim() == "")
            {
                throw new ArgumentException("Shader retrieved empty source");
            }

            GL.ShaderSource(ID, shaderSource);

            return this;
        }

        /// <summary>
        /// Load shader from file.
        /// </summary>
        /// <param name="path">File path</param>
        /// <returns>self</returns>
        public Shader SetShaderFile(string path)
        {
            SetShaderSource(File.ReadAllText(path));
            return this; 
        }

        /// <summary>
        /// Compile shader.
        /// </summary>
        /// <returns>self</returns>
        public Shader Compile()
        {
            GL.CompileShader(ID);

            GL.GetShader(ID, ShaderParameter.CompileStatus, out var code);

            if (code != (int)All.True)
            {
                var infoLog = GL.GetShaderInfoLog(ID);
                throw new Exception($"Error occurred whilst compiling Shader ({ID}).\n\nLog: {infoLog}");
            }

            return this;
        }

        /// <summary>
        /// Delete shader.
        /// </summary>
        /// <returns>self</returns>
        public Shader Delete()
        {
            GL.DeleteShader(ID);
            return this;
        }


    }
}
