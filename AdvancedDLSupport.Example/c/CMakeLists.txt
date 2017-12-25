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

add_library(Demo SHARED src/Example.c ${SHARED_HEADERS})

get_filename_component(INSTALL_PATH_ABSOLUTE "${PROJECT_PATH}/${OUTPUT_PATH}" ABSOLUTE)
install(TARGETS Demo
        DESTINATION ${INSTALL_PATH_ABSOLUTE})