# Document Converter Security Hardening Plan

1. **Add upload size limits**
   - Set a maximum request/file size in Nginx.
   - Add the same or stricter limit in ASP.NET Core.
   - Reject oversized files before any parsing or conversion begins.

2. **Validate allowed file types**
   - Only accept `.docx` and `.odt`.
   - Do not trust the file extension or `Content-Type` alone.
   - Verify that the uploaded file is actually a ZIP-based document with the expected internal structure.

3. **Add archive bomb protection**
   - Limit the maximum number of ZIP entries.
   - Limit the maximum size of each decompressed entry.
   - Limit the maximum total decompressed size.
   - Optionally reject extremely high compression ratios.
   - Stop processing immediately when a limit is exceeded.

4. **Protect against Zip Slip / path traversal**
   - Never blindly extract paths supplied by the archive.
   - Resolve every destination path and verify that it remains inside the temporary working directory.
   - Reject entries containing unsafe paths such as `../`.

5. **Use safe temporary files and directories**
   - Generate server-side random names instead of trusting uploaded filenames.
   - Give each conversion its own temporary directory.
   - Delete the directory after conversion succeeds or fails.
   - Ensure temporary files cannot overwrite application files.

6. **Add stricter DOCX/ODT structure validation**
   - For DOCX, verify required files such as `[Content_Types].xml` and `word/document.xml`.
   - For ODT, verify expected files such as `mimetype` and `content.xml`.
   - Reject malformed archives before passing them further into the system.

7. **Add a LibreOffice conversion timeout**
   - Set a maximum conversion duration, e.g. 20–30 seconds.
   - Kill the LibreOffice process tree if it exceeds the limit.
   - Clean up temporary files afterwards.

8. **Limit concurrent conversions**
   - Prevent unlimited LibreOffice processes from running simultaneously.
   - Use a semaphore, queue, or similar mechanism in the API.
   - Start with a small number such as 2–4 concurrent conversions for the VPS.

9. **Restrict container resources**
   - Add Docker CPU and memory limits for the API/conversion container.
   - Prevent one malicious document from consuming all VPS resources.
   - Consider process/PID limits as well.

10. **Run the application as a non-root user**
   - Configure the Docker container so ASP.NET and LibreOffice do not run as root.
   - Give the application access only to directories it actually needs.

11. **Restrict filesystem access**
   - Keep uploaded files inside dedicated temporary storage.
   - Keep application secrets, SQLite data, and Data Protection keys inaccessible to LibreOffice where possible.
   - Consider a read-only container filesystem with writable temp directories.

12. **Restrict network access during conversion**
   - Prevent LibreOffice from making unnecessary outbound network requests.
   - Ideally isolate the actual conversion process/container from the network entirely.

13. **Add API rate limiting**
   - Apply per-user/IP limits to `/api/document`.
   - Limit how frequently users can start conversions.
   - Use stricter limits for expensive conversion endpoints than for normal API requests.

14. **Strengthen authentication protection**
   - Keep the conversion endpoint authenticated.
   - Add rate limiting to login attempts.
   - Ensure failed authentication attempts cannot be used to generate excessive server work.

15. **Handle malformed documents safely**
   - Catch ZIP, XML, document-processing, and LibreOffice errors.
   - Return generic `400`/`422` responses rather than internal exception details.
   - Never expose filesystem paths, stack traces, or server configuration to users.

16. **Add XML parser protections**
   - Disable external entity resolution where applicable.
   - Prevent XML from loading external resources.
   - Add reasonable limits for XML/document complexity where possible.

17. **Add logging for suspicious uploads**
   - Log rejected oversized files, malformed archives, archive-limit violations, timeouts, and rate-limit events.
   - Do not log uploaded document contents or sensitive variables.
   - Include enough information to identify repeated abuse.

18. **Add automatic cleanup**
   - Ensure temporary files are deleted in a `finally` block.
   - Also periodically remove abandoned temporary directories in case the server crashes during conversion.

19. **Create security tests**
   - Test oversized uploads.
   - Test fake `.docx` files.
   - Test ZIP bombs / highly compressed archives.
   - Test archives with thousands of entries.
   - Test `../` path traversal.
   - Test malformed XML.
   - Test documents that cause conversion timeouts.
   - Test many simultaneous conversion requests.

20. **Separate conversion into stronger isolation later**
   - Longer-term, consider moving LibreOffice conversion into its own worker/container.
   - API accepts and validates the file → isolated worker converts it → API returns the result.
   - This creates a stronger security boundary if the service becomes more widely used.

## Suggested implementation order

Implement **steps 1–8 first**. These cover most of the immediate practical risks around malicious uploads and resource exhaustion.

Then complete **steps 9–19** as the hardening phase.

Treat **step 20** as a longer-term architectural improvement if the service grows.
