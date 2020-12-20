package com.abyssaldev.abyss.interactions

import com.fasterxml.jackson.annotation.JsonIgnore
import com.fasterxml.jackson.annotation.JsonIgnoreProperties
import com.fasterxml.jackson.annotation.JsonProperty
import com.fasterxml.jackson.annotation.JsonRawValue

@JsonIgnoreProperties(ignoreUnknown = true)
class Interaction {
    lateinit var id: String
    var type: Int? = null

    var member: InteractionMember? = null

    @JsonProperty("guild_id")
    var guildId: String? = ""

    @JsonProperty("channel_id")
    var channelId: String? = ""

    @JsonProperty("token")
    var responseToken: String? = ""

    @JsonProperty("version")
    var version: Int? = null

    var data: InteractionData? = null
}

@JsonIgnoreProperties(ignoreUnknown = true)
class InteractionMember {
    lateinit var roles: Array<String>
    lateinit var user: InteractionUser
}

@JsonIgnoreProperties(ignoreUnknown = true)
class InteractionUser {
    lateinit var id: String
    lateinit var username: String
    lateinit var avatar: String
    lateinit var discriminator: String
}

@JsonIgnoreProperties(ignoreUnknown = true)
class InteractionData {
    lateinit var id: String
    lateinit var name: String
    var options: Array<InteractionCommandOption>? = null
}

@JsonIgnoreProperties(ignoreUnknown = true)
class InteractionCommandOption {
    lateinit var name: String
    @JsonRawValue
    lateinit var value: String
}