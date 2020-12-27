package com.abyssaldev.abyss.util

import com.fasterxml.jackson.annotation.JsonIgnore
import org.slf4j.Logger
import org.slf4j.LoggerFactory

interface Loggable {
    @get:JsonIgnore
    val logger: Logger
        get() = LoggerFactory.getLogger(this.javaClass.simpleName)

    @JsonIgnore
    fun getCustomLogger(name: String) = LoggerFactory.getLogger(name)

    @JsonIgnore
    fun getCustomLogger(clazz: Class<*>) = LoggerFactory.getLogger(clazz)
}