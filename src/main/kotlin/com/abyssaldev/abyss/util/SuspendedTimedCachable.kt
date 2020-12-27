package com.abyssaldev.abyss.util

import java.time.Duration
import java.time.Instant

class SuspendedTimedCachable<T>(val fetch: suspend () -> T, val cacheLengthMillis: Long) : Loggable {
    private var cached: T? = null
    private var lastCacheTime: Instant? = null

    suspend fun get(): T {
        if (cached != null || lastCacheTime == null || (Duration.between(Instant.now(), lastCacheTime!!)).toMillis() > cacheLengthMillis) {
            cached = fetch()
            lastCacheTime = Instant.now()
        }
        return cached!!
    }
}