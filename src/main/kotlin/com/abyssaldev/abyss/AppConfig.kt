package com.abyssaldev.abyss

import com.abyssaldev.abyss.util.Loggable
import com.abyssaldev.abyss.util.parseHex
import com.abyssaldev.abyss.util.read
import com.abyssaldev.abyss.util.write
import com.fasterxml.jackson.annotation.JsonIgnore
import com.fasterxml.jackson.annotation.JsonProperty
import java.awt.Color
import java.io.File

class AppConfig: Loggable {
    companion object {
        val file: File by lazy {
            File("appconfig.json")
        }
        val instance: AppConfig by lazy {
            AbyssEngine.jsonEngine.read<AppConfig>(file).apply {
                this.logger.info("Created AppConfig instance from appconfig.json.")
            }
        }
    }

    @JsonIgnore
    fun writeToDisk() {
        if (!file.canWrite()) {
            return logger.info("Attempted to write config to disk, but file is not writable.")
        }
        file.writeText(AbyssEngine.jsonEngine.write(this, true))
        logger.info("Wrote appconfig.json to disk.")
    }

    var discord: AppConfigDiscord = AppConfigDiscord()
    var web: AppConfigWeb = AppConfigWeb()
    var appearance: AppConfigAppearance = AppConfigAppearance()
    val keys: AppConfigApis = AppConfigApis()

    @JsonProperty("command_scopes")
    var commandScopes: HashMap<String, String> = HashMap()

    @JsonIgnore
    fun determineCommandScope(name: String): String? {
        if (!commandScopes["all"].isNullOrEmpty()) return commandScopes["all"]!!
        return commandScopes[name]
    }

    class AppConfigDiscord {
        lateinit var botToken: String
        lateinit var interactionsPublicKey: String
        lateinit var ownerId: String
        var gatewayPrefix: String = "/gw "
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