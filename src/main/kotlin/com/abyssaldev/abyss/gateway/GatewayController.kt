package com.abyssaldev.abyss.gateway

import com.abyssaldev.abyss.AbyssEngine
import com.abyssaldev.abyss.AppConfig
import com.abyssaldev.abyss.persistence.AbyssGatewayGuildSettingsManager
import com.abyssaldev.abyss.util.Loggable

class GatewayController: Loggable {
    val commandClient: GatewayCommandClient = GatewayCommandClientBuilder().apply {
        setPrefix("/gw ")
        useHelpBuilder(false)
        setGuildSettingsManager(AbyssGatewayGuildSettingsManager())
        setOwnerId(AppConfig.instance.discord.ownerId)
        setActivity(AbyssEngine.globalActivity)
    }.build()

    val readyListenerAdapter = GatewayReadyListenerAdapter()

    val listeners = arrayOf(commandClient, readyListenerAdapter)
}