// <copyright file="GoogleSignIn.cs" company="Google Inc.">
// Copyright (C) 2017 Google Inc. All Rights Reserved.
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// </copyright>

namespace Google {
  using System;
  using System.Runtime.Serialization;
  using System.Threading.Tasks;
  using Google.Impl;
  using UnityEngine;

  /// <summary>
  /// Google sign in API.
  /// </summary>
  /// <remarks>This class implements the GoogleSignInAPI for Unity.
  /// Typical usage is to set the Configuration options as needed, then
  /// get the DefaultInstance and call signIn or signInSilently.  See
  /// the <a href="https://developers.google.com/identity">
  /// Google Sign-In API documentation for more details.</a>
  /// <para>
  /// <code>
  /// private static readonly GoogleSignInConfiguration configuration =
  ///                        new GoogleSignInConfiguration {
  ///                          WebClientId = "<your client id here >",
  ///                          RequestIdToken = true
  ///                        };
  ///
  /// public void OnSignIn() {
  ///   GoogleSignIn.Configuration = configuration;
  ///   GoogleSignIn.Configuration.UseGameSignIn = false;
  ///   GoogleSignIn.Configuration.RequestIdToken = true;
  ///   GoogleSignIn.DefaultInstance.SignIn().ContinueWith(
  ///           OnAuthenticationFinished);
  /// }
  /// </code>
  /// </para>
  /// </remarks>
  public class GoogleSignIn : IDisposable {

#if !UNITY_ANDROID && !UNITY_IOS
  static GoogleSignIn() {
    Debug.LogError("This platform is not supported");
  }
#endif

    private ISignInImpl impl;
    public static GoogleSignIn Create(GoogleSignInConfiguration configuration)
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        return new GoogleSignIn(new GoogleSignInImplEditor(configuration));
#elif UNITY_ANDROID || UNITY_IOS
        return new GoogleSignIn(new GoogleSignInImpl(configuration));
#else
        throw new SignInException(
            GoogleSignInStatusCode.DEVELOPER_ERROR,
            "This platform is not supported by GoogleSignIn");
#endif
    }

    internal GoogleSignIn(ISignInImpl impl) {
      this.impl = impl;
    }

    public void EnableDebugLogging(bool flag) {
            impl.EnableDebugLogging(flag);
    }

    /// <summary>Starts the authentication process.</summary>
    /// <remarks>
    /// The authenication process is started and may display account picker
    /// popups and consent prompts based on the state of authentication and
    /// the requested elements.
    /// </remarks>
    public Task<GoogleSignInUser> SignIn() {
      var tcs = new TaskCompletionSource<GoogleSignInUser>();
      SignInHelperObject.Instance.StartCoroutine(
        impl.SignIn().WaitForResult(tcs));
      return tcs.Task;
    }

    public Task<GoogleSignInUser> SignInAsync() {
      var tcs = new TaskCompletionSource<GoogleSignInUser>();
      impl.SignIn().WaitForResultAsync(tcs);
      return tcs.Task;
    }

    internal Future<GoogleSignInUser> SignInFuture() => impl.SignIn();

    /// <summary>Starts the silent authentication process.</summary>
    /// <remarks>
    /// The authenication process is started and will attempt to sign in without
    /// displaying any UI.  If this cannot be done, the developer should call
    /// SignIn().
    /// </remarks>
    public Task<GoogleSignInUser> SignInSilently() {
      var tcs = new TaskCompletionSource<GoogleSignInUser>();
      SignInHelperObject.Instance.StartCoroutine(
          impl.SignInSilently().WaitForResult(tcs));
      return tcs.Task;
    }

    public Task<GoogleSignInUser> SignInSilentlyAsync() {
      var tcs = new TaskCompletionSource<GoogleSignInUser>();
      impl.SignInSilently().WaitForResultAsync(tcs);
      return tcs.Task;
    }

    internal Future<GoogleSignInUser> SignInSilentlyFuture() => impl.SignInSilently();

    /// <summary>
    /// Signs out the User.
    /// </summary>
    /// <remarks>Future sign-in attempts will require the user to select the
    /// account to use when signing in.
    /// </remarks>
    public void SignOut() {
      impl.SignOut();
    }

    /// <summary>
    /// Disconnect this instance.
    /// </summary>
    /// <remarks>When the user is disconnected, it revokes all access that may
    /// have been granted to this application.  This includes any server side
    /// access tokens derived from server auth codes.  As a result, future
    /// sign-in attempts will require the user to re-consent to the requested
    /// scopes.
    /// </remarks>
    public void Disconnect() {
      impl.Disconnect();
    }

    /// <summary>
    /// Sign in exception.  This is a checked exception for handling specific
    /// errors during the sign-in process.
    /// </summary>
    [Serializable]
    public class SignInException : Exception {
      internal SignInException(GoogleSignInStatusCode status) {
        Status = status;
      }

      public SignInException(GoogleSignInStatusCode status, string message) :
          base(message) {
        Status = status;
      }

      public SignInException(GoogleSignInStatusCode status, string message,
          Exception innerException) : base(message, innerException) {
        Status = status;
      }

      protected SignInException(GoogleSignInStatusCode status,
                                SerializationInfo info,
                                StreamingContext context) :
          base(info, context) {
        Status = status;
      }

      public GoogleSignInStatusCode Status {
        get;
        internal set;
      }
    }

    public void Dispose()
    {
        if (impl is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
  }

  internal interface ISignInImpl {
    Future<GoogleSignInUser> SignIn();
    Future<GoogleSignInUser> SignInSilently();
    void EnableDebugLogging(bool flag);
    void SignOut();
    void Disconnect();
  }
}  // namespace Google
