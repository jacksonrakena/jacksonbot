package com.abyssaldev.abyss.util

import org.slf4j.Logger
import org.slf4j.LoggerFactory

interface Loggable {
    val logger: Logger
        get() = LoggerFactory.getLogger(this.javaClass.simpleName)

    fun getCustomLogger(name: String) = LoggerFactory.getLogger(name)
    fun getCustomLogger(clazz: Class<*>) = LoggerFactory.getLogger(clazz)
}