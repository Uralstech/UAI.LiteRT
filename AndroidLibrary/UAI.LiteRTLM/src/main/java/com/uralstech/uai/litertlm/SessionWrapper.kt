// Copyright 2025 URAV ADVANCED LEARNING SYSTEMS PRIVATE LIMITED
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

package com.uralstech.uai.litertlm

import android.util.Log
import com.google.ai.edge.litertlm.InputData
import com.google.ai.edge.litertlm.ResponseCallback
import com.google.ai.edge.litertlm.Session
import java.util.concurrent.Executors

class SessionWrapper(private val session: Session) {

    companion object {
        private const val TAG = "SessionWrapper"
    }

    interface AsyncDecodeCallback {
        fun onDone(result: String)
    }

    private val executor = Executors.newSingleThreadExecutor()

    fun isAlive() : Boolean {
        return session.isAlive
    }

    fun runPrefill(input: List<InputData>) : Boolean {
        if (!checkSession()) return false

        session.runPrefill(input)
        Log.i(TAG, "Input prefilled (sync).")
        return true
    }

    fun runPrefillAsync(input: List<InputData>, onDone: Runnable) : Boolean {
        if (!checkSession()) return false

        Log.i(TAG, "Running prefill (async).")
        executor.submit {
            session.runPrefill(input)
            Log.i(TAG, "Input prefilled (async).")

            onDone.run()
        }

        return true
    }

    fun runDecode() : String? {
        if (!checkSession()) return null

        val result = session.runDecode()
        Log.i(TAG, "Decoded output (sync).")
        return result
    }

    fun runDecodeAsync(callback: AsyncDecodeCallback) : Boolean {
        if (!checkSession()) return false

        Log.i(TAG, "Decoding output (async).")
        executor.submit {
            val result = session.runDecode()
            Log.i(TAG, "Decoded output (async).")

            callback.onDone(result)
        }

        return true
    }

    fun generateContent(input: List<InputData>) : String? {
        if (!checkSession()) return null

        val result = session.generateContent(input)
        Log.i(TAG, "Content generated (sync).")
        return result
    }

    fun generateContentStream(input: List<InputData>, responseCallback: ResponseCallback) : Boolean {
        if (!checkSession()) return false

        Log.i(TAG, "Generating content (async).")
        session.generateContentStream(input, responseCallback)
        return true
    }

    fun cancelProcess() : Boolean {
        if (!checkSession()) return false

        session.cancelProcess()
        Log.i(TAG, "Process cancelled.")
        return true
    }

    private fun checkSession() : Boolean {
        if (!session.isAlive) {
            Log.e(TAG, "Tried to use dead session!")
            return false
        }

        return true
    }

    fun close() {
        if (session.isAlive) {
            session.close()
            Log.i(TAG, "Session closed.")
        }

        if (!executor.isShutdown) {
            executor.shutdown()
            Log.i(TAG, "Session executor shut down.")
        }
    }
}