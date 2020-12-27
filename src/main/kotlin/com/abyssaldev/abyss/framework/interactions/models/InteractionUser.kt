package com.abyssaldev.abyss.framework.interactions.models

import com.fasterxml.jackson.annotation.JsonIgnoreProperties

@JsonIgnoreProperties(ignoreUnknown = true)
class InteractionUser {
    lateinit var id: String
    lateinit var username: String
    lateinit var avatar: String
    lateinit var discriminator: String
}