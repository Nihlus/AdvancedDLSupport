function(install_for_frameworks FRAMEWORKS)
    foreach(framework ${FRAMEWORKS})
        message("Framework: ${framework}")

        install(TARGETS
                    ${StandardTargets}
                COMPONENT
                    standard
                DESTINATION
                    ${INSTALL_PATH_ABSOLUTE}/${framework}
                OPTIONAL)

        install(TARGETS
                    ${x64Targets}
                COMPONENT
                    x64-only
                DESTINATION
                    ${INSTALL_PATH_ABSOLUTE}/${framework}/lib/x64
                OPTIONAL)

        install(TARGETS
                    ${x32Targets}
                COMPONENT
                    x32-only
                DESTINATION
                    ${INSTALL_PATH_ABSOLUTE}/${framework}/lib/x32
                OPTIONAL)

    endforeach()
endfunction()
