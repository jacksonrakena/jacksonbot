package com.abyssaldev.abyss.framework.interactions.models

import com.fasterxml.jackson.annotation.JsonIgnoreProperties

@JsonIgnoreProperties(ignoreUnknown = true)
class InteractionMember {
    lateinit var roles: Array<String>
    lateinit var user: InteractionUser
}