package com.abyssaldev.abyss.util

import java.time.Duration
import java.time.Instant

/**
 * A cacheable object container that is lazily initialized and expires after a certain time.
 */
class TimedCacheable<T>(val fetch: () -> T, cacheLengthMillis: Long) : CacheableBase<T>(cacheLengthMillis),  Loggable {
    /**
     * Returns the value of this instance. This function will fetch the value using the [TimedCacheable.fetch]
     * generator if the value is not cached, or has expired.
     */
    fun get(): T {
        if (isExpired) {
            cached = fetch()
            lastCacheTime = Instant.now()
        }
        return cached!!
    }

    /**
     * A cacheable object container that has a suspending function as its generator.
     */
    class Suspended<T>(val fetch: suspend () -> T, cacheLengthMillis: Long) : CacheableBase<T>(cacheLengthMillis), Loggable {
        /**
         * Returns the value of this instance. This function will fetch the value using the suspendable
         * [TimedCacheable.Suspended.fetch] function if the value is not cached, or has expired.
         */
        suspend fun get(): T {
            if (isExpired) {
                cached = fetch()
                lastCacheTime = Instant.now()
            }
            return cached!!
        }
    }
}

open class CacheableBase<T>(private val cacheLengthMillis: Long) {
    protected var cached: T? = null
    protected var lastCacheTime: Instant? = null

    /**
     * Returns `true` if the internal value is null, or expired.
     */
    val isExpired: Boolean
        get() {
            if (cached != null || lastCacheTime == null || (Duration.between(Instant.now(), lastCacheTime!!)).toMillis() > cacheLengthMillis) {
                return true
            }
            return false
        }
}