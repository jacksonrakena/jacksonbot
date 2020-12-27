package com.abyssaldev.abyss.framework.gateway

import com.abyssaldev.abyss.AbyssEngine
import com.abyssaldev.abyss.AppConfig
import com.abyssaldev.abyss.commands.gateway.AdminCommandSet
import com.abyssaldev.abyss.persistence.AbyssGatewayGuildSettingsManager
import com.abyssaldev.abyss.util.Loggable
import net.dv8tion.jda.api.JDABuilder

class GatewayController: Loggable {
    val commandClient: GatewayCommandClient = GatewayCommandClientBuilder().apply {
        setPrefix(AppConfig.instance.discord.gatewayPrefix)
        useHelpBuilder(false)
        setGuildSettingsManager(AbyssGatewayGuildSettingsManager())
        setOwnerId(AppConfig.instance.discord.ownerId)
        setActivity(AbyssEngine.globalActivity)
        setEmojis(":ballot_box_with_check:", ":warning:", ":x:")

        registerCommands()
    }.build()

    val readyListenerAdapter = GatewayReadyListenerAdapter()

    fun applyListeners(jdaBuilder: JDABuilder) {
        jdaBuilder.addEventListeners(commandClient, readyListenerAdapter)
    }

    private fun GatewayCommandClientBuilder.registerCommands() = apply {
        addCommands(
            AdminCommandSet()
        )
        logger.info("All gateway commands registered.")
    }
}