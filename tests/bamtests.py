from testconfigurations import TestSetup, visualc, visualc64, visualc32, mingw32, gcc, gcc64, gcc32, clang, clang32, clang64

def configure_repository():
    configs = {}
    configs.setdefault("zeromqtest", [])
    configs["zeromqtest"] = TestSetup(win={"Native": [visualc64, visualc32, mingw32], "VSSolution": [visualc64, visualc32], "MakeFile": [visualc64, visualc32, mingw32]},
                                      linux={"Native": [gcc64, gcc32], "MakeFile": [gcc64, gcc32]},
                                      osx={"Native": [clang64, clang32], "MakeFile": [clang64, clang32], "Xcode": [clang64, clang32]},
                                      options="--zeromq.version=3.2.3",
                                      alias="zeromq3.2.3")
    configs["zeromqtest"] = TestSetup(win={"Native": [visualc64, visualc32, mingw32], "VSSolution": [visualc64, visualc32], "MakeFile": [visualc64, visualc32, mingw32]},
                                      linux={"Native": [gcc64, gcc32], "MakeFile": [gcc64, gcc32]},
                                      osx={"Native": [clang64, clang32], "MakeFile": [clang64, clang32], "Xcode": [clang64, clang32]},
                                      options="--zeromq.version=4.1.3",
                                      alias="zeromq4.1.3")
    return configs
