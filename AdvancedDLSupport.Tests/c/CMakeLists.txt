cmake_minimum_required(VERSION 3.5)
project(c)

set(CMAKE_C_STANDARD 11)
set(CMAKE_CXX_STANDARD 11)

set(CMAKE_GENERATOR_PLATFORM x64)

if ( CMAKE_COMPILER_IS_GNUCC )
	set(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} -Wall -Wno-long-long -pedantic")
    set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -Wall -Wno-long-long -pedantic")
endif()
if ( MSVC )
	set(CMAKE_CXX_FLAGS "${CMAKE_C_FLAGS} /W4")
    set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} /W4")
endif()

set(OUTPUT_PATH "${CMAKE_BINARY_DIR}/lib" CACHE STRING "The path where the output binaries should be installed.")
set(PROJECT_PATH "${CMAKE_BINARY_DIR}/lib" CACHE STRING "The path where the output binaries should be installed.")

include_directories(${CMAKE_SOURCE_DIR})

set(SHARED_HEADERS src/TestStruct.h src/wincomp.h)

add_library(BaseTests SHARED src/BaseTests.c ${SHARED_HEADERS})
add_library(DisposeTests SHARED src/DisposeTests.c ${SHARED_HEADERS})
add_library(EventTests SHARED src/EventTests.c ${SHARED_HEADERS})
add_library(FunctionTests SHARED src/FunctionTests.c ${SHARED_HEADERS})
add_library(LazyLoadingTests SHARED src/LazyLoadingTests.c ${SHARED_HEADERS})
add_library(PropertyTests SHARED src/PropertyTests.c ${SHARED_HEADERS})
add_library(RemappingTests SHARED src/RemappingTests.c ${SHARED_HEADERS})
add_library(ComplexTypeTests SHARED src/ComplexTypeTests.c ${SHARED_HEADERS})

get_filename_component(INSTALL_PATH_ABSOLUTE "${PROJECT_PATH}/${OUTPUT_PATH}" ABSOLUTE)

install(TARGETS BaseTests DisposeTests EventTests FunctionTests LazyLoadingTests PropertyTests RemappingTests
        DESTINATION ${INSTALL_PATH_ABSOLUTE})