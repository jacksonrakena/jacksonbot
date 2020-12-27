package com.abyssaldev.abyss.persistence

import com.abyssaldev.abyss.framework.gateway.GatewayGuildSettingsManager
import net.dv8tion.jda.api.entities.Guild

class AbyssGatewayGuildSettingsManager : GatewayGuildSettingsManager<GatewayGuildSettings> {
    override fun getSettings(guild: Guild?): GatewayGuildSettings? = null
}