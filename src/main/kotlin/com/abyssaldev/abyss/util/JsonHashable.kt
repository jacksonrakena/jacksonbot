package com.abyssaldev.abyss.util

import java.util.*

/**
 * Represents an object that can be mapped to a JSON object (HashMap<String, Any>)
 */
interface JsonHashable {

    /**
     * Creates a JSON map (a HashMap<String, Any>) that represents the data transfer object
     * for this instance.
     */
    fun toJsonMap(): JsonMap

    fun <T: JsonHashable> Array<T>.toJsonArray(): List<JsonMap> = this.map(JsonHashable::toJsonMap)

    fun <T: JsonHashable> List<T>.toJsonArray(): List<JsonMap> = this.map(JsonHashable::toJsonMap)
}

typealias JsonMap = HashMap<String, Any>