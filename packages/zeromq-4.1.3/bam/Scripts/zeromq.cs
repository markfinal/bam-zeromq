#region License
// Copyright (c) 2010-2019, Mark Final
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
    [Bam.Core.ModuleGroup("Thirdparty/ZeroMQ")]
    class SDK :
        C.SDKTemplate
    {
        protected override Bam.Core.TypeArray LibraryModuleTypes { get; } = new Bam.Core.TypeArray(
            typeof(ZMQSharedLibrary)
        );

        protected override bool HonourHeaderFileLayout => false;
    }

    [Bam.Core.ModuleGroup("Thirdparty/ZeroMQ")]
    class ZMQSharedLibrary :
        C.Cxx.DynamicLibrary,
        C.IPublicHeaders
    {
        Bam.Core.TokenizedString C.IPublicHeaders.SourceRootDirectory { get; } = null;

        Bam.Core.StringArray C.IPublicHeaders.PublicHeaderPaths { get; } = new Bam.Core.StringArray(
            "include/zmq.h",
            "include/zmq_utils.h"
        );

        protected override void
        Init()
        {
            base.Init();

            this.SetSemanticVersion(4, 1, 3);

            this.CreateHeaderCollection("$(packagedir)/include/*.h");

            var source = this.CreateCxxSourceCollection("$(packagedir)/src/*.cpp");

            source.PrivatePatch(settings =>
                {
                    if (settings is C.ICommonCompilerSettings compiler)
                    {
                        compiler.WarningsAsErrors = false;
                    }

                    if (settings is C.ICxxOnlyCompilerSettings cxxCompiler)
                    {
                        cxxCompiler.ExceptionHandler = C.Cxx.EExceptionHandler.Asynchronous;
                    }

                    var preprocessor = settings as C.ICommonPreprocessorSettings;
                    if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
                    {
                        preprocessor.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/builds/msvc"));
                        // Note: this appears from the CMakeLists now
                        preprocessor.PreprocessorDefines.Add("ZMQ_USE_SELECT");
                    }

                    if (settings is VisualCCommon.ICommonCompilerSettings vcCompiler)
                    {
                        vcCompiler.WarningLevel = VisualCCommon.EWarningLevel.Level4;
                    }

                    /*
                    preprocessor.PreprocessorDefines.Add("DLL_EXPORT");

                    var cxxCompiler = settings as C.ICxxOnlyCompilerSettings;
                    cxxCompiler.ExceptionHandler = C.Cxx.EExceptionHandler.Synchronous;

                    if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
                    {
                        if (settings is VisualCCommon.ICommonCompilerSettings vcCompiler)
                        {
                            vcCompiler.WarningLevel = VisualCCommon.EWarningLevel.Level2;
                        }
                    }

                    if (settings is ClangCommon.ICommonCompilerSettings clangCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.Add("unused-parameter"); // zeromq-4.1.3/src/plain_client.cpp:146:30: error: unused parameter 'cmd_data' [-Werror,-Wunused-parameter]
                    }

                    if (settings is GccCommon.ICommonCompilerSettings gccCompiler)
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.DisableWarnings.Add("unused-parameter");
                    }
                    */
                });
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.OSX | Bam.Core.EPlatform.Linux))
            {
                // TODO: is there a call for a CompileWith function?
                var platformHeader = Bam.Core.Graph.Instance.FindReferencedModule<ZMQPlatformHeader>();
                source.DependsOn(platformHeader);
                source.UsePublicPatches(platformHeader);
                // TODO: end of function

                /*
                source.PrivatePatch(settings =>
                {
                    var preprocessor = settings as C.ICommonPreprocessorSettings;
                    preprocessor.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagebuilddir)/$(config)"));
                });

                if (this.Linker is ClangCommon.LinkerBase)
                {
                    source["ipc_listener.cpp"].ForEach(item =>
                        {
                            item.PrivatePatch(settings =>
                                {
                                    var compiler = settings as C.ICommonCompilerSettings;
                                    compiler.DisableWarnings.Add("deprecated-declarations");
                                });
                        });
                }
                */
            }

            /*
            this.PublicPatch((settings, appliedTo) =>
                {
                    if (settings is C.ICommonPreprocessorSettings preprocessor)
                    {
                        preprocessor.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/include"));
                    }
                });
                */

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
                    /*
                    else if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Linux))
                    {
                        linker.Libraries.Add("-lpthread");
                    }
                    */
                });
        }
    }
}
