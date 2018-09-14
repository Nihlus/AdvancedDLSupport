cmake_minimum_required(VERSION 3.5)
project(c)

include(CMake/install-functions.cmake)

set(CMAKE_C_STANDARD 11)
set(CMAKE_CXX_STANDARD 11)

set(BUILD_PLATFORM "x64" CACHE STRING "The target build platform.")
set(TARGET_FRAMEWORKS "net461" CACHE STRING "The target frameworks")
set(BASE_INSTALL_PATH "${CMAKE_BINARY_DIR}/lib" CACHE STRING "The base path (not including the framework where the output binaries should be installed.")
set(PROJECT_PATH "${CMAKE_BINARY_DIR}/lib" CACHE STRING "The base path of the project.")

if (CMAKE_COMPILER_IS_GNUCC OR CMAKE_C_COMPILER_ID MATCHES "Clang")
    set(IS_UNIX_COMPILER true)
endif()

if (IS_UNIX_COMPILER)
	set(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} -Wall -Wno-long-long -pedantic")
    set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -Wall -Wno-long-long -pedantic")
elseif ( MSVC )
	set(CMAKE_CXX_FLAGS "${CMAKE_C_FLAGS} /W4 /MP")
    set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} /W4 /MP")
endif()


include_directories(${CMAKE_SOURCE_DIR})

set(SHARED_HEADERS src/comp.h)

add_library(AOTTests SHARED src/AOTTests.c ${SHARED_HEADERS})

if (IS_UNIX_COMPILER)
    # Any CPU is assumed to be 64-bit
    if (BUILD_PLATFORM STREQUAL "x64" OR BUILD_PLATFORM STREQUAL "AnyCPU")
        set_target_properties(AOTTests PROPERTIES COMPILE_FLAGS "-m64" LINK_FLAGS "-m64")
    elseif (BUILD_PLATFORM STREQUAL "x86")
        set_target_properties(AOTTests PROPERTIES COMPILE_FLAGS "-m32" LINK_FLAGS "-m32")
    endif()
elseif (MSVC)
    # Handled in MSBuild
endif()

get_filename_component(INSTALL_PATH_ABSOLUTE "${PROJECT_PATH}/${BASE_INSTALL_PATH}" ABSOLUTE)

install_for_frameworks("${TARGET_FRAMEWORKS}")
