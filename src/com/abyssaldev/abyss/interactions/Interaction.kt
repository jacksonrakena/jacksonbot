package com.abyssaldev.abyss.interactions

import com.fasterxml.jackson.annotation.JsonIgnoreProperties

@JsonIgnoreProperties(ignoreUnknown = true)
class Interaction {
    lateinit var id: String
    var type: Int? = null
}