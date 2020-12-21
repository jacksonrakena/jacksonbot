package com.abyssaldev.abyss

import java.io.File

class AppConfig {
    companion object {
        val configFile = File("appconfig.json")

        val instance: AppConfig by lazy {
            AbyssEngine.objectMapper.readValue(configFile, AppConfig::class.java)
        }
    }

    lateinit var discord: AppConfigDiscord
    lateinit var web: AppConfigWeb

    class AppConfigDiscord {
        lateinit var botToken: String
        lateinit var interactionsPublicKey: String
    }

    class AppConfigWeb {
        var port: Int = 80
    }
}