#region License
// Copyright (c) 2010-2017, Mark Final
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of BuildAMation nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion // License
using Bam.Core;
namespace zeromq
{
    public class ZMQPlatformHeader :
        C.ProceduralHeaderFile
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);
            this.Macros.Add("templateConfig", this.CreateTokenizedString("$(packagedir)/src/platform.hpp.in"));
        }

        protected override TokenizedString OutputPath
        {
            get
            {
                return this.CreateTokenizedString("$(packagebuilddir)/$(config)/platform.hpp");
            }
        }

        protected override string Contents
        {
            get
            {
                var contents = new System.Text.StringBuilder();

                // parse the input header, and modify it while writing it out
                // modifications are platform specific
                using (System.IO.TextReader readFile = new System.IO.StreamReader(this.Macros["templateConfig"].ToString()))
                {
                    var destPath = this.GeneratedPaths[Key].ToString();
                    var destDir = System.IO.Path.GetDirectoryName(destPath);
                    if (!System.IO.Directory.Exists(destDir))
                    {
                        System.IO.Directory.CreateDirectory(destDir);
                    }
                    string line;
                    while ((line = readFile.ReadLine()) != null)
                    {
                        if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.OSX))
                        {
                            // change #undefs to #defines
                            // some need to have a non-zero value, rather than just be defined
                            if (line.Contains("#undef ZMQ_HAVE_OSX") ||
                                line.Contains("#undef ZMQ_HAVE_UIO"))
                            {
                                var split = line.Split(new[] { ' ' });
                                contents.AppendFormat("#define {0} 1", split[1]);
                                contents.AppendLine();
                                continue;
                            }
                        }
                        else if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Linux))
                        {
                            // change #undefs to #defines
                            // some need to have a non-zero value, rather than just be defined
                            if (line.Contains("#undef ZMQ_HAVE_LINUX") ||
                                line.Contains("#undef ZMQ_HAVE_UIO"))
                            {
                                var split = line.Split(new[] { ' ' });
                                contents.AppendFormat("#define {0} 1", split[1]);
                                contents.AppendLine();
                                continue;
                            }
                        }
                        contents.AppendLine(line);
                    }
                    if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.OSX))
                    {
                        contents.AppendLine("#define ZMQ_USE_KQUEUE");
                    }
                    else if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Linux))
                    {
                        contents.AppendLine("#define ZMQ_USE_EPOLL");
                    }
                }

                return contents.ToString();
            }
        }
    }
}
