(require 'json)

(defconst quarry--modifier-prefixes '("C-" "M-" "S-" "A-" "H-" "s-"))

(defun quarry--mods-string (mods)
  (string-join mods "|"))

(defun quarry--map-mod (prefix)
  (pcase prefix
    ("C-" "Control")
    ("M-" "Alt")
    ("A-" "Alt")
    ("S-" "Shift")
    ("s-" "Meta")
    ("H-" "Meta")))

(defun quarry--normalize-key (key)
  (cond
   ((= (length key) 1) (upcase key))
   ((member (upcase key) '("SPC" "SPACE")) "SPC")
   ((member (upcase key) '("RET" "RETURN")) "RET")
   ((string= (upcase key) "TAB") "TAB")
   ((member (upcase key) '("ESC" "ESCAPE")) "ESC")
   ((member (upcase key) '("DEL" "DELETE")) "DEL")
   (t key)))

(defun quarry--match-mod-prefix (token)
  (catch 'found
    (dolist (prefix quarry--modifier-prefixes)
      (when (string-prefix-p prefix token)
        (throw 'found (cons prefix (substring token (length prefix))))))
    nil))

(defun quarry--parse-description (desc)
  (let ((token desc)
        (mods '())
        (match t))
    (while match
      (setq match (quarry--match-mod-prefix token))
      (when match
        (push (quarry--map-mod (car match)) mods)
        (setq token (cdr match))))
    (if mods
        (list 'chord (nreverse mods) (quarry--normalize-key token))
      (list 'plain (quarry--normalize-key token)))))

(defun quarry--event-description (code)
  (cond
   ((integerp code) (single-key-description code))
   ((symbolp code) (symbol-name code))
   (t (prin1-to-string code))))

(defun quarry--wire-from-env ()
  (let ((wire (getenv "QUARRY_ORACLE_WIRE")))
    (when (and wire (not (string= wire "")))
      wire)))

(defun quarry--wire-from-argv ()
  (let ((args argv))
    (when (and args (string= (car args) "--"))
      (setq args (cdr args)))
    (mapconcat 'identity args " ")))

(defun quarry--key-parse (wire)
  (condition-case nil
      (key-parse wire t)
    (wrong-number-of-arguments
      (key-parse wire))))

(defun quarry--wire-steps (wire)
  (let* ((parsed (quarry--key-parse wire))
         (steps '()))
    (dotimes (i (length parsed))
      (push (quarry--parse-description
             (quarry--event-description (aref parsed i)))
            steps))
    (nreverse steps)))

(defun quarry--step-alist (step)
  (let ((kind (car step)))
    (if (eq kind 'plain)
        `((kind . "plain") (key . ,(cadr step)))
      `((kind . "chord")
        (mods . ,(quarry--mods-string (cadr step)))
        (key . ,(caddr step))))))

(defun quarry--main ()
  (let* ((wire (or (quarry--wire-from-env) (quarry--wire-from-argv)))
         (steps (mapcar #'quarry--step-alist (quarry--wire-steps wire))))
    (princ (json-encode `((wire . ,wire) (steps . ,steps))))))

(quarry--main)
