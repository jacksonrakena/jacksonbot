package com.abyssaldev.abyss.framework.interactions.arguments

import com.fasterxml.jackson.annotation.JsonIgnoreProperties
import com.fasterxml.jackson.annotation.JsonRawValue

@JsonIgnoreProperties(ignoreUnknown = true)
class InteractionCommandArgumentChoice {
    var name: String = ""
    @JsonRawValue
    var value: String = ""

    var options: Array<InteractionCommandArgumentChoice> = emptyArray()
}