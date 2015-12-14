from testconfigurations import TestSetup, visualc64, mingw32, gcc64, clang64

def configure_repository():
    configs = {}
    configs["zeromqtest"] = TestSetup(win={"Native":[visualc64,mingw32],"VSSolution":[visualc64],"MakeFile":[visualc64,mingw32]},
                                      linux={"Native":[gcc64],"MakeFile":[gcc64]},
                                      osx={"Native":[clang64],"MakeFile":[clang64],"Xcode":[clang64]})
    return configs
