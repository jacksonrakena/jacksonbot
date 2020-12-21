package com.abyssaldev.abyss.interactions.abs

import java.util.*

/**
 * Represents an object that can be mapped to a JSON object (HashMap<String, Any>)
 */
interface JsonHashable {
    /**
     * Creates a JSON map (a HashMap<String, Any>) that represents the data transfer object
     * for this instance.
     */
    fun createMap(): HashMap<String, Any>
}