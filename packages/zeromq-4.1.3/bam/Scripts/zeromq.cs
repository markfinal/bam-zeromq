#region License
// Copyright (c) 2010-2015, Mark Final
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
using System.Linq;
namespace zeromq
{
    [Bam.Core.ModuleGroup("Thirdparty/ZeroMQ")]
    public sealed class ZMQSharedLibrary :
        C.Cxx.DynamicLibrary
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.Macros["MajorVersion"] = Bam.Core.TokenizedString.CreateVerbatim("4");
            this.Macros["MinorVersion"] = Bam.Core.TokenizedString.CreateVerbatim("1");
            this.Macros["PatchVersion"] = Bam.Core.TokenizedString.CreateVerbatim("3");

            this.CreateHeaderContainer("$(packagedir)/include/*.h");

            this.Macros.Add("zmqsrcdir", this.CreateTokenizedString("$(packagedir)/src"));
            var source = this.CreateCxxSourceContainer("$(zmqsrcdir)/*.cpp", macroModuleOverride: this);

            source.PrivatePatch(settings =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.PreprocessorDefines.Add("DLL_EXPORT");

                    var cxxCompiler = settings as C.ICxxOnlyCompilerSettings;
                    cxxCompiler.ExceptionHandler = C.Cxx.EExceptionHandler.Synchronous;

                    if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
                    {
                        compiler.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/builds/msvc"));

                        // Note: this appears from the CMakeLists now
                        compiler.PreprocessorDefines.Add("ZMQ_USE_SELECT");

                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            vcCompiler.WarningLevel = VisualCCommon.EWarningLevel.Level2;
                        }
                    }

                    var clangCompiler = settings as ClangCommon.ICommonCompilerSettings;
                    if (null != clangCompiler)
                    {
                        compiler.DisableWarnings.Add("unused-parameter"); // zeromq-4.1.3/src/plain_client.cpp:146:30: error: unused parameter 'cmd_data' [-Werror,-Wunused-parameter]
                    }

                    var gccCompiler = settings as GccCommon.ICommonCompilerSettings;
                    if (null != gccCompiler)
                    {
                        compiler.DisableWarnings.Add("unused-parameter");
                    }
                });
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.OSX | Bam.Core.EPlatform.Linux))
            {
                // TODO: is there a call for a CompileWith function?
                var platformHeader = Bam.Core.Graph.Instance.FindReferencedModule<ZMQPlatformHeader>();
                source.DependsOn(platformHeader);
                source.UsePublicPatches(platformHeader);
                // TODO: end of function

                source.PrivatePatch(settings =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagebuilddir)/$(config)"));
                });

                if (this.Linker is ClangCommon.LinkerBase)
                {
                    var ipc_listener = source.Children.Where(item => item.InputPath.Parse().EndsWith("ipc_listener.cpp"));
                    ipc_listener.ElementAt(0).PrivatePatch(settings =>
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.Add("deprecated-declarations");
                    });
                }
            }

            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows) &&
                this.Linker is VisualCCommon.LinkerBase)
            {
                this.CompilePubliclyAndLinkAgainst<WindowsSDK.WindowsSDK>(source);
            }

            this.PublicPatch((settings, appliedTo) =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    if (null != compiler)
                    {
                        compiler.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/include"));
                    }
                });

            this.PrivatePatch(settings =>
                {
                    var linker = settings as C.ICommonLinkerSettings;
                    if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
                    {
                        if (this.Linker is VisualCCommon.LinkerBase)
                        {
                            linker.Libraries.Add("Ws2_32.lib");
                            linker.Libraries.Add("Advapi32.lib");
                        }
                        else if (this.Linker is MingwCommon.LinkerBase)
                        {
                            linker.Libraries.Add("-lws2_32");
                            linker.Libraries.Add("-ladvapi32");
                        }
                    }
                    else if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Linux))
                    {
                        linker.Libraries.Add("-lpthread");
                    }
                });
        }
    }
}
