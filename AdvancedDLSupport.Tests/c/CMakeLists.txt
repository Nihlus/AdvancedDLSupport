cmake_minimum_required(VERSION 3.7)
project(c)

include(CMake/install-functions.cmake)

set(CMAKE_C_STANDARD 11)
set(CMAKE_CXX_STANDARD 11)

set(BUILD_PLATFORM "x64" CACHE STRING "The target build platform.")
set(TARGET_FRAMEWORKS "netcoreapp2.0" "net461" CACHE STRING "The target frameworks")
set(BASE_INSTALL_PATH "${CMAKE_BINARY_DIR}/lib" CACHE STRING "The base path (not including the framework where the output binaries should be installed.")
set(PROJECT_PATH "${CMAKE_BINARY_DIR}/lib" CACHE STRING "The base path of the project.")

if (CMAKE_COMPILER_IS_GNUCC OR CMAKE_C_COMPILER_ID MATCHES "Clang")
    set(IS_UNIX_COMPILER true)
endif()

if (IS_UNIX_COMPILER)
	set(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} -Wall -Wno-long-long -pedantic")
    set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -Wall -Wno-long-long -pedantic")
elseif ( MSVC )
	set(CMAKE_CXX_FLAGS "${CMAKE_C_FLAGS} /W4")
    set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} /W4")
endif()

list(
    APPEND StandardTargets
        BaseTests
        DisposeTests
        EventTests
        FunctionTests
        LazyLoadingTests
        PropertyTests
        RemappingTests
        StringTests
        AttributePassthroughTests
        MixedModeTests
        NullableTests
        IndirectCallTests
        NameManglingTests
        SymbolTransformationTests
        GenericDelegateTests
        BooleanMarshallingTests
        GenericFunctionTests
)

list(
    APPEND x32Targets
        BaseTests-x32
)


list(
    APPEND x64Targets
        BaseTests-x64
)

include_directories(${CMAKE_SOURCE_DIR})

set(SHARED_HEADERS src/TestStruct.h src/comp.h)

# Uncomment if testing on a non-unicode platform
add_definitions(-DUNICODE)

foreach (target ${StandardTargets})
    add_library(${target} SHARED src/${target}.c ${SHARED_HEADERS})
endforeach(target)

foreach (x32Target ${x32Targets})
    string(REGEX MATCH "[^-]+" baseName ${x32Target})

    add_library(${x32Target} SHARED src/${baseName}.c ${SHARED_HEADERS})
endforeach(x32Target)

foreach (x64Target ${x64Targets})
    string(REGEX MATCH "[^-]+" baseName ${x64Target})

    add_library(${x64Target} SHARED src/${baseName}.c ${SHARED_HEADERS})
endforeach(x64Target)

if (IS_UNIX_COMPILER)
    # Any CPU is assumed to be 64-bit
    if (NOT BUILD_PLATFORM STREQUAL "x86")
        foreach (target ${StandardTargets})
            set_target_properties(${target} PROPERTIES COMPILE_FLAGS "-m64" LINK_FLAGS "-m64")
        endforeach(target)
        set_target_properties(BooleanMarshallingTests PROPERTIES COMPILE_FLAGS "-m64" LINK_FLAGS "-m64")
    elseif (BUILD_PLATFORM STREQUAL "x86")
        foreach (target ${StandardTargets})
            set_target_properties(${target} PROPERTIES COMPILE_FLAGS "-m32" LINK_FLAGS "-m32")
        endforeach(target)
        set_target_properties(BooleanMarshallingTests PROPERTIES COMPILE_FLAGS "-m32" LINK_FLAGS "-m32")
    endif()

    foreach (x64Target ${x64Targets})
        set_target_properties(${x64Target} PROPERTIES COMPILE_FLAGS "-m64" LINK_FLAGS "-m64")
    endforeach(x64Target)

    foreach (x32Target ${x32Targets})
        set_target_properties(${x32Target} PROPERTIES COMPILE_FLAGS "-m32" LINK_FLAGS "-m32")
    endforeach(x32Target)
elseif (MSVC)
    # Handled in MSBuild
endif()

get_filename_component(INSTALL_PATH_ABSOLUTE "${PROJECT_PATH}/${BASE_INSTALL_PATH}" ABSOLUTE)

install_for_frameworks("${TARGET_FRAMEWORKS}")
