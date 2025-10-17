from PIL import Image
import os

def resize_image_with_transparency(input_path, output_path, max_width=800):
    """
    Resize an image while preserving transparency
    """
    # Open the original image
    img = Image.open(input_path)

    # Convert to RGBA if not already (ensures transparency channel)
    if img.mode != 'RGBA':
        img = img.convert('RGBA')

    # Calculate new dimensions (maintain aspect ratio)
    original_width, original_height = img.size
    if original_width <= max_width:
        # Image is already small enough, just optimize
        resized_img = img
    else:
        aspect_ratio = original_height / original_width
        new_width = max_width
        new_height = int(new_width * aspect_ratio)

        # Resize with high quality
        resized_img = img.resize((new_width, new_height), Image.Resampling.LANCZOS)

    # Save with transparency preserved
    resized_img.save(output_path, 'PNG', optimize=True)

    # Get file sizes
    original_size = os.path.getsize(input_path)
    resized_size = os.path.getsize(output_path)

    print(f'Original: {original_width}x{original_height}, {original_size:,} bytes ({original_size/1024/1024:.2f} MB)')
    print(f'Resized: {resized_img.size[0]}x{resized_img.size[1]}, {resized_size:,} bytes ({resized_size/1024/1024:.2f} MB)')
    print(f'Reduction: {(1 - resized_size/original_size)*100:.1f}%')
    print(f'Mode: {resized_img.mode} (should be RGBA for transparency)')

    # Close images
    img.close()
    resized_img.close()

# Process the main image
input_path = 'E:/Tiny_Walnut_Games/TheStoryTest/img.png'
output_path = 'E:/Tiny_Walnut_Games/TheStoryTest/img_resized.png'

resize_image_with_transparency(input_path, output_path)