package com.abyssaldev.abyss

import com.abyssaldev.abyss.util.parseHex
import com.abyssaldev.abyss.util.read
import com.fasterxml.jackson.annotation.JsonIgnore
import com.fasterxml.jackson.annotation.JsonProperty
import java.awt.Color
import java.io.File

class AppConfig {
    companion object {
        val instance: AppConfig by lazy {
            AbyssEngine.jsonEngine.read(File("appconfig.json"))
        }
    }

    var discord: AppConfigDiscord = AppConfigDiscord()
    var web: AppConfigWeb = AppConfigWeb()
    var appearance: AppConfigAppearance = AppConfigAppearance()
    val keys: AppConfigApis = AppConfigApis()

    @JsonProperty("command_scopes")
    var commandScopes: HashMap<String, String> = HashMap()

    fun determineCommandScope(name: String): String? {
        if (!commandScopes["all"].isNullOrEmpty()) return commandScopes["all"]!!
        return commandScopes[name]
    }

    class AppConfigDiscord {
        lateinit var botToken: String
        lateinit var interactionsPublicKey: String
        lateinit var ownerId: String
    }

    class AppConfigWeb {
        var port: Int = 80
        var interactionsRoute: String = "/discord/interactions"
    }

    class AppConfigAppearance {
        var defaultEmbedColor: String = "#8edeaa"

        @get:JsonIgnore
        val defaultEmbedColorObject: Color? by lazy(defaultEmbedColor::parseHex)
    }

    class AppConfigApis {
        lateinit var exchangeRateApiKey: String
    }
}