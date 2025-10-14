from transformers import AutoTokenizer
from transformers.utils.chat_template_utils import render_chat_template

tokenizer = AutoTokenizer.from_pretrained("TheBloke/Mistral-7B-Instruct-v0.1-AWQ")

messages = [
    {"role": "system", "content": "You are a helpful assistant."},
    {"role": "user", "content": "What is the capital of France?"}
]

rendered = render_chat_template(messages, tokenizer.chat_template, tokenize=False)
print(rendered)