package com.abyssaldev.abyss

import com.fasterxml.jackson.databind.ObjectMapper
import java.io.File

class AppConfig {
    companion object {
        val objectMapper = ObjectMapper()
        val configFile = File("appconfig.json")

        val instance: AppConfig by lazy {
            objectMapper.readValue(configFile, AppConfig::class.java)
        }
    }

    lateinit var discord: AppConfigDiscord
}

class AppConfigDiscord {
    lateinit var botToken: String
    lateinit var interactionsPublicKey: String
}