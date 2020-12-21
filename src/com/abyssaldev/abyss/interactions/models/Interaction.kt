package com.abyssaldev.abyss.interactions.models

import com.fasterxml.jackson.annotation.JsonIgnoreProperties
import com.fasterxml.jackson.annotation.JsonProperty

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

