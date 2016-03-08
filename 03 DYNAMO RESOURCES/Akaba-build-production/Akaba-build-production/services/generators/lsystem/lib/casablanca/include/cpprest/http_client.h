/***
* ==++==
*
* Copyright (c) Microsoft Corporation. All rights reserved.
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*
* ==--==
* =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
*
* HTTP Library: Client-side APIs.
*
* For the latest on this and related APIs, please see http://casablanca.codeplex.com.
*
* =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
****/
#pragma once

#ifndef _CASA_HTTP_CLIENT_H
#define _CASA_HTTP_CLIENT_H

#if defined (__cplusplus_winrt)
#define __WRL_NO_DEFAULT_LIB__
#include <wrl.h>
#include <msxml6.h>
namespace web { namespace http{namespace client{
typedef IXMLHTTPRequest2* native_handle;}}}
#else
namespace web { namespace http{namespace client{
typedef void* native_handle;}}}
#endif // __cplusplus_winrt

#include <memory>
#include <limits>

#include "pplx/pplxtasks.h"
#include "cpprest/http_msg.h"
#include "cpprest/json.h"
#include "cpprest/uri.h"
#include "cpprest/details/web_utilities.h"
#include "cpprest/details/basic_types.h"
#include "cpprest/asyncrt_utils.h"

#if !defined(CPPREST_TARGET_XP) && (!defined(WINAPI_FAMILY) || WINAPI_FAMILY != WINAPI_FAMILY_PHONE_APP || _MSC_VER > 1700)
#include "cpprest/oauth1.h"
#endif

#include "cpprest/oauth2.h"

/// The web namespace contains functionality common to multiple protocols like HTTP and WebSockets.
namespace web
{
/// Declarations and functionality for the HTTP protocol.
namespace http
{
/// HTTP client side library.
namespace client
{

// credentials and web_proxy class has been moved from web::http::client namespace to web namespace.
// The below using declarations ensure we don't break existing code.
// Please use the web::credentials and web::web_proxy class going forward.
using web::credentials;
using web::web_proxy;

#ifdef _WIN32
namespace details {
#ifdef __cplusplus_winrt
        class winrt_client ;
#else
        class winhttp_client;
#endif // __cplusplus_winrt
}
#endif // _WIN32

/// <summary>
/// HTTP client configuration class, used to set the possible configuration options
/// used to create an http_client instance.
/// </summary>
class http_client_config
{
public:
    http_client_config() :
        m_guarantee_order(false),
        m_timeout(utility::seconds(30)),
        m_chunksize(0)
#if !defined(__cplusplus_winrt)
        , m_validate_certificates(true)
#endif
        , m_set_user_nativehandle_options([](native_handle)->void{})
#if defined(_WIN32) && !defined(__cplusplus_winrt)
        , m_buffer_request(false)
#endif
    {
    }

#if !defined(CPPREST_TARGET_XP) && (!defined(WINAPI_FAMILY) || WINAPI_FAMILY != WINAPI_FAMILY_PHONE_APP || _MSC_VER > 1700)
    /// <summary>
    /// Get OAuth 1.0 configuration.
    /// </summary>
    /// <returns>Shared pointer to OAuth 1.0 configuration.</returns>
    const std::shared_ptr<oauth1::experimental::oauth1_config> oauth1() const
    {
        return m_oauth1;
    }

    /// <summary>
    /// Set OAuth 1.0 configuration.
    /// </summary>
    /// <param name="config">OAuth 1.0 configuration to set.</param>
    void set_oauth1(oauth1::experimental::oauth1_config config)
    {
        m_oauth1 = std::make_shared<oauth1::experimental::oauth1_config>(std::move(config));
    }
#endif

    /// <summary>
    /// Get OAuth 2.0 configuration.
    /// </summary>
    /// <returns>Shared pointer to OAuth 2.0 configuration.</returns>
    const std::shared_ptr<oauth2::experimental::oauth2_config> oauth2() const
    {
        return m_oauth2;
    }

    /// <summary>
    /// Set OAuth 2.0 configuration.
    /// </summary>
    /// <param name="config">OAuth 2.0 configuration to set.</param>
    void set_oauth2(oauth2::experimental::oauth2_config config)
    {
        m_oauth2 = std::make_shared<oauth2::experimental::oauth2_config>(std::move(config));
    }

    /// <summary>
    /// Get the web proxy object
    /// </summary>
    /// <returns>A reference to the web proxy object.</returns>
    const web_proxy& proxy() const
    {
        return m_proxy;
    }

    /// <summary>
    /// Set the web proxy object
    /// </summary>
    /// <param name="proxy">A reference to the web proxy object.</param>
    void set_proxy(web_proxy proxy)
    {
        m_proxy = std::move(proxy);
    }

    /// <summary>
    /// Get the client credentials
    /// </summary>
    /// <returns>A reference to the client credentials.</returns>
    const http::client::credentials& credentials() const
    {
        return m_credentials;
    }

    /// <summary>
    /// Set the client credentials
    /// </summary>
    /// <param name="cred">A reference to the client credentials.</param>
    void set_credentials(const http::client::credentials& cred)
    {
        m_credentials = cred;
    }

    /// <summary>
    /// Get the 'guarantee order' property
    /// </summary>
    /// <returns>The value of the property.</returns>
    bool guarantee_order() const
    {
        return m_guarantee_order;
    }

    /// <summary>
    /// Set the 'guarantee order' property
    /// </summary>
    /// <param name="guarantee_order">The value of the property.</param>
    CASABLANCA_DEPRECATED("Confusing API will be removed in future releases. If you need to order HTTP requests use task continuations.")
    void set_guarantee_order(bool guarantee_order)
    {
        m_guarantee_order = guarantee_order;
    }

    /// <summary>
    /// Get the timeout
    /// </summary>
    /// <returns>The timeout (in seconds) used for each send and receive operation on the client.</returns>
    utility::seconds timeout() const
    {
        return m_timeout;
    }

    /// <summary>
    /// Set the timeout
    /// </summary>
    /// <param name="timeout">The timeout (in seconds) used for each send and receive operation on the client.</param>
    void set_timeout(const utility::seconds &timeout)
    {
        m_timeout = timeout;
    }

    /// <summary>
    /// Get the client chunk size.
    /// </summary>
    /// <returns>The internal buffer size used by the http client when sending and receiving data from the network.</returns>
    size_t chunksize() const
    {
        return m_chunksize == 0 ? 64 * 1024 : m_chunksize;
    }

    /// <summary>
    /// Sets the client chunk size.
    /// </summary>
    /// <param name="size">The internal buffer size used by the http client when sending and receiving data from the network.</param>
    /// <remarks>This is a hint -- an implementation may disregard the setting and use some other chunk size.</remarks>
    void set_chunksize(size_t size)
    {
        m_chunksize = size;
    }

    /// <summary>
    /// Returns true if the default chunk size is in use.
    /// <remarks>If true, implementations are allowed to choose whatever size is best.</remarks>
    /// </summary>
    /// <returns>True if default, false if set by user.</returns>
    bool is_default_chunksize() const
    {
        return m_chunksize == 0;
    }

#if !defined(__cplusplus_winrt)
    /// <summary>
    /// Gets the server certificate validation property.
    /// </summary>
    /// <returns>True if certificates are to be verified, false otherwise.</returns>
    bool validate_certificates() const
    {
        return m_validate_certificates;
    }

    /// <summary>
    /// Sets the server certificate validation property.
    /// </summary>
    /// <param name="validate_certs">False to turn ignore all server certificate validation errors, true otherwise.</param>
    /// <remarks>Note ignoring certificate errors can be dangerous and should be done with caution.</remarks>
    void set_validate_certificates(bool validate_certs)
    {
        m_validate_certificates = validate_certs;
    }
#endif

#ifdef _WIN32
#if !defined(__cplusplus_winrt)
    /// <summary>
    /// Checks if request data buffering is turned on, the default is off.
    /// </summary>
    /// <returns>True if buffering is enabled, false otherwise</returns>
    bool buffer_request() const
    {
        return m_buffer_request;
    }

    /// <summary>
    /// Sets the request buffering property.
    /// If true, in cases where the request body/stream doesn't support seeking the request data will be buffered.
    /// This can help in situations where an authentication challenge might be expected.
    /// </summary>
    /// <param name="buffer_request">True to turn on buffer, false otherwise.</param>
    /// <remarks>Please note there is a performance cost due to copying the request data.</remarks>
    void set_buffer_request(bool buffer_request)
    {
        m_buffer_request = buffer_request;
    }
#endif
#endif

    /// <summary>
    /// Sets a callback to enable custom setting of winhttp options
    /// </summary>
    /// <param name="callback">A user callback allowing for customization of the request</param>
    void set_nativehandle_options(const std::function<void(native_handle)> &callback)
    {
         m_set_user_nativehandle_options = callback;
    }

private:
#if !defined(CPPREST_TARGET_XP) && (!defined(WINAPI_FAMILY) || WINAPI_FAMILY != WINAPI_FAMILY_PHONE_APP || _MSC_VER > 1700)
    std::shared_ptr<oauth1::experimental::oauth1_config> m_oauth1;
#endif

    std::shared_ptr<oauth2::experimental::oauth2_config> m_oauth2;
    web_proxy m_proxy;
    http::client::credentials m_credentials;
    // Whether or not to guarantee ordering, i.e. only using one underlying TCP connection.
    bool m_guarantee_order;

    utility::seconds m_timeout;
    size_t m_chunksize;

#if !defined(__cplusplus_winrt)
    // IXmlHttpRequest2 doesn't allow configuration of certificate verification.
    bool m_validate_certificates;
#endif

    std::function<void(native_handle)> m_set_user_nativehandle_options;

#if defined(_WIN32) && defined(__cplusplus_winrt)
    friend class details::winrt_client;
#elif defined(_WIN32)
    bool m_buffer_request;

    friend class details::winhttp_client;
#endif // defined(_WIN32) && defined(__cplusplus_winrt)

    /// <summary>
    /// Invokes a user callback to allow for customization of the request
    /// </summary>
    /// <param name="handle">The internal http_request handle</param>
    /// <returns>True if users set WinHttp/IXAMLHttpRequest2 options correctly, false otherwise.</returns>
    void call_user_nativehandle_options(native_handle handle) const
    {
         m_set_user_nativehandle_options(handle);
    }
};

/// <summary>
/// HTTP client class, used to maintain a connection to an HTTP service for an extended session.
/// </summary>
class http_client
{
public:
    /// <summary>
    /// Creates a new http_client connected to specified uri.
    /// </summary>
    /// <param name="base_uri">A string representation of the base uri to be used for all requests. Must start with either "http://" or "https://"</param>
    _ASYNCRTIMP http_client(const uri &base_uri);

    /// <summary>
    /// Creates a new http_client connected to specified uri.
    /// </summary>
    /// <param name="base_uri">A string representation of the base uri to be used for all requests. Must start with either "http://" or "https://"</param>
    /// <param name="client_config">The http client configuration object containing the possible configuration options to initialize the <c>http_client</c>. </param>
    _ASYNCRTIMP http_client(const uri &base_uri, const http_client_config &client_config);

    /// <summary>
    /// Note the destructor doesn't necessarily close the connection and release resources.
    /// The connection is reference counted with the http_responses.
    /// </summary>
    ~http_client() CPPREST_NOEXCEPT {}

    /// <summary>
    /// Gets the base URI.
    /// </summary>
    /// <returns>
    /// A base URI initialized in constructor
    /// </returns>
    _ASYNCRTIMP const uri& base_uri() const;

    /// <summary>
    /// Get client configuration object
    /// </summary>
    /// <returns>A reference to the client configuration object.</returns>
    _ASYNCRTIMP const http_client_config& client_config() const;

    /// <summary>
    /// Adds an HTTP pipeline stage to the client.
    /// </summary>
    /// <param name="handler">A function object representing the pipeline stage.</param>
    void add_handler(std::function<pplx::task<http_response>(http_request, std::shared_ptr<http::http_pipeline_stage>)> handler)
    {
        m_pipeline->append(std::make_shared<::web::http::details::function_pipeline_wrapper>(handler));
    }

    /// <summary>
    /// Adds an HTTP pipeline stage to the client.
    /// </summary>
    /// <param name="stage">A shared pointer to a pipeline stage.</param>
    void add_handler(const std::shared_ptr<http::http_pipeline_stage> &stage)
    {
        m_pipeline->append(stage);
    }

    /// <summary>
    /// Asynchronously sends an HTTP request.
    /// </summary>
    /// <param name="request">Request to send.</param>
    /// <param name="token">Cancellation token for cancellation of this request operation.</param>
    /// <returns>An asynchronous operation that is completed once a response from the request is received.</returns>
    _ASYNCRTIMP pplx::task<http_response> request(http_request request, const pplx::cancellation_token &token = pplx::cancellation_token::none());

    /// <summary>
    /// Asynchronously sends an HTTP request.
    /// </summary>
    /// <param name="mtd">HTTP request method.</param>
    /// <param name="token">Cancellation token for cancellation of this request operation.</param>
    /// <returns>An asynchronous operation that is completed once a response from the request is received.</returns>
    pplx::task<http_response> request(const method &mtd, const pplx::cancellation_token &token = pplx::cancellation_token::none())
    {
        http_request msg(mtd);
        return request(msg, token);
    }

    /// <summary>
    /// Asynchronously sends an HTTP request.
    /// </summary>
    /// <param name="mtd">HTTP request method.</param>
    /// <param name="path_query_fragment">String containing the path, query, and fragment, relative to the http_client's base URI.</param>
    /// <param name="token">Cancellation token for cancellation of this request operation.</param>
    /// <returns>An asynchronous operation that is completed once a response from the request is received.</returns>
    pplx::task<http_response> request(
        const method &mtd,
        const utility::string_t &path_query_fragment,
        const pplx::cancellation_token &token = pplx::cancellation_token::none())
    {
        http_request msg(mtd);
        msg.set_request_uri(path_query_fragment);
        return request(msg, token);
    }

    /// <summary>
    /// Asynchronously sends an HTTP request.
    /// </summary>
    /// <param name="mtd">HTTP request method.</param>
    /// <param name="path_query_fragment">String containing the path, query, and fragment, relative to the http_client's base URI.</param>
    /// <param name="body_data">The data to be used as the message body, represented using the json object library.</param>
    /// <param name="token">Cancellation token for cancellation of this request operation.</param>
    /// <returns>An asynchronous operation that is completed once a response from the request is received.</returns>
    pplx::task<http_response> request(
        const method &mtd,
        const utility::string_t &path_query_fragment,
        const json::value &body_data,
        const pplx::cancellation_token &token = pplx::cancellation_token::none())
    {
        http_request msg(mtd);
        msg.set_request_uri(path_query_fragment);
        msg.set_body(body_data);
        return request(msg, token);
    }

    /// <summary>
    /// Asynchronously sends an HTTP request with a string body. Assumes the
    /// character encoding of the string is UTF-8.
    /// </summary>
    /// <param name="mtd">HTTP request method.</param>
    /// <param name="path_query_fragment">String containing the path, query, and fragment, relative to the http_client's base URI.</param>
    /// <param name="content_type">A string holding the MIME type of the message body.</param>
    /// <param name="body_data">String containing the text to use in the message body.</param>
    /// <param name="token">Cancellation token for cancellation of this request operation.</param>
    /// <returns>An asynchronous operation that is completed once a response from the request is received.</returns>
    pplx::task<http_response> request(
        const method &mtd,
        const utf8string &path_query_fragment,
        const utf8string &body_data,
        const utf8string &content_type = "text/plain; charset=utf-8",
        const pplx::cancellation_token &token = pplx::cancellation_token::none())
    {
        http_request msg(mtd);
        msg.set_request_uri(::utility::conversions::to_string_t(path_query_fragment));
        msg.set_body(body_data, content_type);
        return request(msg, token);
    }

    /// <summary>
    /// Asynchronously sends an HTTP request with a string body. Assumes the
    /// character encoding of the string is UTF-8.
    /// </summary>
    /// <param name="mtd">HTTP request method.</param>
    /// <param name="path_query_fragment">String containing the path, query, and fragment, relative to the http_client's base URI.</param>
    /// <param name="content_type">A string holding the MIME type of the message body.</param>
    /// <param name="body_data">String containing the text to use in the message body.</param>
    /// <param name="token">Cancellation token for cancellation of this request operation.</param>
    /// <returns>An asynchronous operation that is completed once a response from the request is received.</returns>
    pplx::task<http_response> request(
        const method &mtd,
        const utf8string &path_query_fragment,
        utf8string &&body_data,
        const utf8string &content_type = "text/plain; charset=utf-8",
        const pplx::cancellation_token &token = pplx::cancellation_token::none())
    {
        http_request msg(mtd);
        msg.set_request_uri(::utility::conversions::to_string_t(path_query_fragment));
        msg.set_body(std::move(body_data), content_type);
        return request(msg, token);
    }

    /// <summary>
    /// Asynchronously sends an HTTP request with a string body. Assumes the
    /// character encoding of the string is UTF-16 will perform conversion to UTF-8.
    /// </summary>
    /// <param name="mtd">HTTP request method.</param>
    /// <param name="path_query_fragment">String containing the path, query, and fragment, relative to the http_client's base URI.</param>
    /// <param name="content_type">A string holding the MIME type of the message body.</param>
    /// <param name="body_data">String containing the text to use in the message body.</param>
    /// <param name="token">Cancellation token for cancellation of this request operation.</param>
    /// <returns>An asynchronous operation that is completed once a response from the request is received.</returns>
    pplx::task<http_response> request(
        const method &mtd,
        const utf16string &path_query_fragment,
        const utf16string &body_data,
        const utf16string &content_type = ::utility::conversions::to_utf16string("text/plain"),
        const pplx::cancellation_token &token = pplx::cancellation_token::none())
    {
        http_request msg(mtd);
        msg.set_request_uri(::utility::conversions::to_string_t(path_query_fragment));
        msg.set_body(body_data, content_type);
        return request(msg, token);
    }

    /// <summary>
    /// Asynchronously sends an HTTP request with a string body. Assumes the
    /// character encoding of the string is UTF-8.
    /// </summary>
    /// <param name="mtd">HTTP request method.</param>
    /// <param name="path_query_fragment">String containing the path, query, and fragment, relative to the http_client's base URI.</param>
    /// <param name="body_data">String containing the text to use in the message body.</param>
    /// <param name="token">Cancellation token for cancellation of this request operation.</param>
    /// <returns>An asynchronous operation that is completed once a response from the request is received.</returns>
    pplx::task<http_response> request(
        const method &mtd,
        const utf8string &path_query_fragment,
        const utf8string &body_data,
        const pplx::cancellation_token &token)
    {
        return request(mtd, path_query_fragment, body_data, "text/plain; charset=utf-8", token);
    }

    /// <summary>
    /// Asynchronously sends an HTTP request with a string body. Assumes the
    /// character encoding of the string is UTF-8.
    /// </summary>
    /// <param name="mtd">HTTP request method.</param>
    /// <param name="path_query_fragment">String containing the path, query, and fragment, relative to the http_client's base URI.</param>
    /// <param name="body_data">String containing the text to use in the message body.</param>
    /// <param name="token">Cancellation token for cancellation of this request operation.</param>
    /// <returns>An asynchronous operation that is completed once a response from the request is received.</returns>
    pplx::task<http_response> request(
        const method &mtd,
        const utf8string &path_query_fragment,
        utf8string &&body_data,
        const pplx::cancellation_token &token)
    {
        http_request msg(mtd);
        msg.set_request_uri(::utility::conversions::to_string_t(path_query_fragment));
        msg.set_body(std::move(body_data), "text/plain; charset=utf-8");
        return request(msg, token);
    }

    /// <summary>
    /// Asynchronously sends an HTTP request with a string body. Assumes
    /// the character encoding of the string is UTF-16 will perform conversion to UTF-8.
    /// </summary>
    /// <param name="mtd">HTTP request method.</param>
    /// <param name="path_query_fragment">String containing the path, query, and fragment, relative to the http_client's base URI.</param>
    /// <param name="body_data">String containing the text to use in the message body.</param>
    /// <param name="token">Cancellation token for cancellation of this request operation.</param>
    /// <returns>An asynchronous operation that is completed once a response from the request is received.</returns>
    pplx::task<http_response> request(
        const method &mtd,
        const utf16string &path_query_fragment,
        const utf16string &body_data,
        const pplx::cancellation_token &token)
    {
        return request(mtd, path_query_fragment, body_data, ::utility::conversions::to_utf16string("text/plain"), token);
    }

#if !defined (__cplusplus_winrt)
    /// <summary>
    /// Asynchronously sends an HTTP request.
    /// </summary>
    /// <param name="mtd">HTTP request method.</param>
    /// <param name="path_query_fragment">String containing the path, query, and fragment, relative to the http_client's base URI.</param>
    /// <param name="body">An asynchronous stream representing the body data.</param>
    /// <param name="content_type">A string holding the MIME type of the message body.</param>
    /// <param name="token">Cancellation token for cancellation of this request operation.</param>
    /// <returns>A task that is completed once a response from the request is received.</returns>
    pplx::task<http_response> request(
        const method &mtd,
        const utility::string_t &path_query_fragment,
        const concurrency::streams::istream &body,
        const utility::string_t &content_type = _XPLATSTR("application/octet-stream"),
        const pplx::cancellation_token &token = pplx::cancellation_token::none())
    {
        http_request msg(mtd);
        msg.set_request_uri(path_query_fragment);
        msg.set_body(body, content_type);
        return request(msg, token);
    }

    /// <summary>
    /// Asynchronously sends an HTTP request.
    /// </summary>
    /// <param name="mtd">HTTP request method.</param>
    /// <param name="path_query_fragment">String containing the path, query, and fragment, relative to the http_client's base URI.</param>
    /// <param name="body">An asynchronous stream representing the body data.</param>
    /// <param name="token">Cancellation token for cancellation of this request operation.</param>
    /// <returns>A task that is completed once a response from the request is received.</returns>
    pplx::task<http_response> request(
        const method &mtd,
        const utility::string_t &path_query_fragment,
        const concurrency::streams::istream &body,
        const pplx::cancellation_token &token)
    {
        return request(mtd, path_query_fragment, body, _XPLATSTR("application/octet-stream"), token);
    }
#endif // __cplusplus_winrt

    /// <summary>
    /// Asynchronously sends an HTTP request.
    /// </summary>
    /// <param name="mtd">HTTP request method.</param>
    /// <param name="path_query_fragment">String containing the path, query, and fragment, relative to the http_client's base URI.</param>
    /// <param name="body">An asynchronous stream representing the body data.</param>
    /// <param name="content_length">Size of the message body.</param>
    /// <param name="content_type">A string holding the MIME type of the message body.</param>
    /// <param name="token">Cancellation token for cancellation of this request operation.</param>
    /// <returns>A task that is completed once a response from the request is received.</returns>
    /// <remarks>Winrt requires to provide content_length.</remarks>
    pplx::task<http_response> request(
        const method &mtd,
        const utility::string_t &path_query_fragment,
        const concurrency::streams::istream &body,
        size_t content_length,
        const utility::string_t &content_type = _XPLATSTR("application/octet-stream"),
        const pplx::cancellation_token &token = pplx::cancellation_token::none())
    {
        http_request msg(mtd);
        msg.set_request_uri(path_query_fragment);
        msg.set_body(body, content_length, content_type);
        return request(msg, token);
    }

    /// <summary>
    /// Asynchronously sends an HTTP request.
    /// </summary>
    /// <param name="mtd">HTTP request method.</param>
    /// <param name="path_query_fragment">String containing the path, query, and fragment, relative to the http_client's base URI.</param>
    /// <param name="body">An asynchronous stream representing the body data.</param>
    /// <param name="content_length">Size of the message body.</param>
    /// <param name="token">Cancellation token for cancellation of this request operation.</param>
    /// <returns>A task that is completed once a response from the request is received.</returns>
    /// <remarks>Winrt requires to provide content_length.</remarks>
    pplx::task<http_response> request(
        const method &mtd,
        const utility::string_t &path_query_fragment,
        const concurrency::streams::istream &body,
        size_t content_length,
        const pplx::cancellation_token &token)
    {
        return request(mtd, path_query_fragment, body, content_length, _XPLATSTR("application/octet-stream"), token);
    }

private:

    void build_pipeline(const uri &base_uri, const http_client_config &client_config);

    std::shared_ptr<::web::http::http_pipeline> m_pipeline;
};

}}}

#endif
