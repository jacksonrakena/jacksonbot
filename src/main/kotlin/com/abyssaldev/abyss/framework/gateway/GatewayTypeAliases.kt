package com.abyssaldev.abyss.framework.gateway

import com.jagrosh.jdautilities.command.CommandClient
import com.jagrosh.jdautilities.command.CommandClientBuilder
import com.jagrosh.jdautilities.command.GuildSettingsManager

typealias GatewayCommandClient = CommandClient
typealias GatewayGuildSettingsManager<T> = GuildSettingsManager<T>
typealias GatewayCommandClientBuilder = CommandClientBuilder