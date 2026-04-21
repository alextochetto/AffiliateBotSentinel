<?php
/**
 * Plugin Name: Bot Sentinel Tracker
 * Description: Sends visitor data to C# API
 */

if (!defined('ABSPATH')) exit;

function csharp_tracker_send() {

    // Get IP (Cloudflare safe)
    $ip = $_SERVER['HTTP_CF_CONNECTING_IP'] ?? $_SERVER['REMOTE_ADDR'] ?? 'unknown';

    // Headers
    $userAgent = $_SERVER['HTTP_USER_AGENT'] ?? 'unknown';

    // Simple bot hint (we’ll improve later)
    $isBotHint = preg_match('/bot|crawl|spider|slurp/i', $userAgent);

    $payload = [
        'ip' => $ip,
        'userAgent' => $userAgent,
        'isBot' => $isBotHint ? true : false,
        'path' => $_SERVER['REQUEST_URI'] ?? '',
        'timestamp' => gmdate('Y-m-d\TH:i:s\Z')
    ];

    wp_remote_post('https://botsentinel.azurewebsites.net/api/TrackBot/Track', [
        'method'   => 'POST',
        'timeout'  => 10,
        'blocking' => false,
        'headers'  => [
            'Content-Type' => 'application/json',
            'trackbot-api-key'    => 'vT9fK2xQ8LmR4Zp7Yw3NcD1Hs6JbA0Ue'
        ],
        'body' => json_encode($payload)
    ]);
}

add_action('init', 'csharp_tracker_send');