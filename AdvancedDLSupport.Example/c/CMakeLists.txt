cmake_minimum_required(VERSION 3.5)
project(c)

set(CMAKE_C_STANDARD 11)
set(CMAKE_CXX_STANDARD 11)

if ( CMAKE_COMPILER_IS_GNUCC )
	set(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} -Wall -Wno-long-long -pedantic")
    set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -Wall -Wno-long-long -pedantic")
endif()
if ( MSVC )
	set(CMAKE_CXX_FLAGS "${CMAKE_C_FLAGS} /W4")
    set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} /W4")
endif()

add_library(Demo SHARED src/Example.c ${SHARED_HEADERS})

install(TARGETS Demo
        DESTINATION ${CMAKE_BINARY_DIR}/lib)