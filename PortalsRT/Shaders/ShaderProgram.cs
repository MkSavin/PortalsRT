using System;
using System.Collections.Generic;
using System.Text;

using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using PortalsRT.PropertyObjects;

namespace PortalsRT.Shaders
{
    public class ShaderProgram
    {
        public int ID { get; private set; }

        public List<Shader> Shaders { get; private set; } = new List<Shader>();

        ShaderProgramStatus status = ShaderProgramStatus.Initialized;
        bool linked = false;

        private Dictionary<string, int> uniforms;

        public ShaderProgram()
        {
            ID = GL.CreateProgram();
        }

        /// <summary>
        /// Add shader to shader program.
        /// </summary>
        /// <param name="shader">Shader</param>
        /// <returns>self</returns>
        public ShaderProgram AddShader(Shader shader)
        {
            Shaders.Add(shader);

            status = ShaderProgramStatus.ShadersAdded;

            return this;
        }

        /// <summary>
        /// Compile shaders of shader program.
        /// </summary>
        /// <returns>self</returns>
        public ShaderProgram CompileShaders()
        {
            if (status != ShaderProgramStatus.ShadersAdded)
            {
                throw new Exception($"Shaders not added to shader program ({ID}) yet");
            }

            foreach (var shader in Shaders)
            {
                shader.Compile();
            }

            status = ShaderProgramStatus.Compiled;

            return this;
        }

        /// <summary>
        /// Attach shaders to shader program.
        /// </summary>
        /// <returns>self</returns>
        public ShaderProgram AttachShaders()
        {
            if (status != ShaderProgramStatus.Compiled)
            {
                throw new Exception($"Shaders not compiled yet (shader program ({ID}))");
            }

            foreach (var shader in Shaders)
            {
                GL.AttachShader(ID, shader.ID);
            }

            status = ShaderProgramStatus.Attached;

            return this;
        }

        /// <summary>
        /// Link shader program.
        /// </summary>
        /// <returns>self</returns>
        public ShaderProgram Link()
        {
            if (status != ShaderProgramStatus.Attached)
            {
                throw new Exception($"Shaders not attached yet (shader program ({ID}))");
            }

            GL.LinkProgram(ID);

            GL.GetProgram(ID, GetProgramParameterName.LinkStatus, out var code);

            if (code != (int)All.True)
            {
                throw new Exception($"Error occurred whilst linking shader program ({ID})");
            }

            linked = true;

            return this;
        }

        /// <summary>
        /// Detach shaders from shader program.
        /// </summary>
        /// <returns>self</returns>
        public ShaderProgram DetachShaders()
        {
            if (status != ShaderProgramStatus.Attached)
            {
                throw new Exception($"Shaders not attached yet (shader program ({ID}))");
            }

            foreach (var shader in Shaders)
            {
                GL.DetachShader(ID, shader.ID);
            }

            status = ShaderProgramStatus.Compiled;

            return this;
        }

        /// <summary>
        /// Delete shaders from cache.
        /// </summary>
        /// <returns>self</returns>
        public ShaderProgram DeleteShaders()
        {
            if (status != ShaderProgramStatus.Compiled)
            {
                throw new Exception($"Shaders not compiled yet (shader program ({ID}))");
            }

            foreach (var shader in Shaders)
            {
                shader.Delete();
            }

            status = ShaderProgramStatus.ShadersAdded;

            return this;
        }

        /// <summary>
        /// Install shaders to program.
        /// </summary>
        /// <returns>self</returns>
        public ShaderProgram InstallShaders()
        {
            CompileShaders();
            AttachShaders();

            return this;
        }

        /// <summary>
        /// Uninstall shaders from program.
        /// </summary>
        /// <returns>self</returns>
        public ShaderProgram UninstallShaders()
        {
            DetachShaders();
            DeleteShaders();

            return this;
        }

        /// <summary>
        /// Update uniforms list.
        /// </summary>
        /// <returns>self</returns>
        public ShaderProgram UpdateUniforms()
        {
            GL.GetProgram(ID, GetProgramParameterName.ActiveUniforms, out var numberOfUniforms);

            uniforms = new Dictionary<string, int>();

            for (var i = 0; i < numberOfUniforms; i++)
            {
                var key = GL.GetActiveUniform(ID, i, out _, out _);
                var location = GL.GetUniformLocation(ID, key);

                uniforms.Add(key, location);
            } 

            return this;
        }

        /// <summary>
        /// Full program bind-process.
        /// </summary>
        /// <returns>self</returns>
        public ShaderProgram FullLink()
        {
            CompileShaders();
            AttachShaders();
            Link();
            DetachShaders();
            DeleteShaders();
            UpdateUniforms();

            return this;
        }

        /// <summary>
        /// Use shader program.
        /// </summary>
        /// <returns>self</returns>
        public ShaderProgram Use()
        {
            if (!linked)
            {
                throw new Exception($"Shaders not linked yet (shader program ({ID}))");
            }

            GL.UseProgram(ID);

            return this;
        }

        /// <summary>
        /// Get shader program attribute location.
        /// </summary>
        /// <param name="attribute">Attribute name</param>
        /// <returns></returns>
        public int GetAttribLocation(string attribute)
        {
            return GL.GetAttribLocation(ID, attribute);
        }

        /// <summary>
        /// Prepare shader program before getting uniform. 
        /// Uses shader program and checks contanment on uniform name in list.
        /// </summary>
        /// <param name="uniform">Uniform name</param>
        private ShaderProgram PrepareProgramForUniform(string uniform)
        {
            if (!uniforms.ContainsKey(uniform))
            {
                throw new ArgumentException($"Uniform with name '{uniform}' not found");
            }

            Use();

            return this;
        }

        /// <summary>
        /// Set a uniform int on this shader program.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        public ShaderProgram SetInt(string name, int data)
        {
            PrepareProgramForUniform(name);
            GL.Uniform1(uniforms[name], data);

            return this;
        }

        /// <summary>
        /// Set a uniform float on this shader program.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        public ShaderProgram SetFloat(string name, float data)
        {
            PrepareProgramForUniform(name);
            GL.Uniform1(uniforms[name], data);

            return this;
        }

        /// <summary>
        /// Set a uniform Matrix4 on this shader program.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        /// <remarks>
        ///   <para>
        ///   The matrix is transposed before being sent to the shader.
        ///   </para>
        /// </remarks>
        public ShaderProgram SetMatrix4(string name, Matrix4 data)
        {
            PrepareProgramForUniform(name);
            GL.UniformMatrix4(uniforms[name], true, ref data);

            return this;
        }

        /// <summary>
        /// Set a uniform Sampler2D on this shader program.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="texture">The data to set</param>
        /// <param name="unit">The texture unit in layout</param>
        public ShaderProgram SetTexture(string name, int texture, TextureUnit unit = TextureUnit.Texture0, int handle = 0)
        {
            PrepareProgramForUniform(name);
            GL.Uniform1(uniforms[name], handle);

            return this;
        }

        /// <summary>
        /// Set a uniform Sampler2D on this shader program.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="texture">The data to set</param>
        /// <param name="unit">The texture unit in layout</param>
        public ShaderProgram SetTexture(string name, Texture texture, TextureUnit unit = TextureUnit.Texture0, int handle = 0)
        {
            PrepareProgramForUniform(name);
            GL.Uniform1(uniforms[name], handle);

            return this;
        }

        /// <summary>
        /// Set a uniform Vector3 on this shader program.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        public ShaderProgram SetVector3(string name, Vector3 data)
        {
            PrepareProgramForUniform(name);
            GL.Uniform3(uniforms[name], data);

            return this;
        }

        /// <summary>
        /// Set a uniform Vector3 on this shader program.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        public ShaderProgram SetFloatArray(string name, List<float> data)
        {
            PrepareProgramForUniform(name);
            GL.Uniform3(uniforms[name], data.Count, data.ToArray());

            return this;
        }

        /// <summary>
        /// Delete shader program from cache.
        /// </summary>
        public void DeleteProgram()
        {
            if (status != ShaderProgramStatus.Compiled && status != ShaderProgramStatus.ShadersAdded)
            {
                UninstallShaders();
            }

            GL.DeleteProgram(ID);
        }
    }
}
