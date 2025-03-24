
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

public class RenderManager : MonoBehaviour
{
    [Header("Asset Libraries")]
    public ShaderLibrary shaderLibrary;
    public MeshLibrary meshLibrary;

    [Header("Scene Data")]
    public List<DrawableMesh> drawables = new();

    [Header("Global Textures")]
    public Texture terrainTexture;
    public Texture pathTexture;
    public Texture blurredPathTexture;

    [Header("Compute Data")]
    public ComputeBuffer indirectTransformsBuffer;

    public Camera mainCamera;

    public void Render()
    {
        if (mainCamera == null) mainCamera = Camera.main;

        Matrix4x4 viewMatrix = mainCamera.worldToCameraMatrix;
        Matrix4x4 projectionMatrix = GL.GetGPUProjectionMatrix(mainCamera.projectionMatrix, false);
        Vector3 cameraPosition = mainCamera.transform.position;

        List<DrawableMesh> transparentPass = new();

        foreach (var drawable in drawables)
        {
            Mesh mesh = meshLibrary.GetMesh(drawable.meshHandle);
            Shader shader = shaderLibrary.GetShaderByHandle(drawable.shaderHandle);
            if (mesh == null || shader == null) continue;

            Material material = new Material(shader);
            MaterialPropertyBlock props = drawable.propertyBlock ?? new MaterialPropertyBlock();

            // Transparent defer
            if (drawable.isTransparent)
            {
                transparentPass.Add(drawable);
                continue;
            }

            // Uniforms
            props.SetMatrix("_Model", drawable.transformMatrix);
            props.SetMatrix("_View", viewMatrix);
            props.SetMatrix("_Projection", projectionMatrix);
            props.SetTexture("_TerrainTex", terrainTexture);

            if (drawable.showPathMask)
                props.SetTexture("_MainTex", blurredPathTexture);

            if (drawable.isRoad)
            {
                props.SetTexture("_PathTex", pathTexture);
                props.SetInt("_IsArch", 1);
                props.SetTexture("_TerrainTex", terrainTexture);
            }

            if (drawable.isIndirectDraw)
            {
                material.SetBuffer("transforms_buffer", indirectTransformsBuffer);
                props.SetInt("_IsArch", 1);

                Graphics.DrawMeshInstancedIndirect(
                    mesh,
                    0,
                    material,
                    drawable.bounds,
                    indirectTransformsBuffer,
                     0,
                    props
                );
                continue;
            }

            if (drawable.instancedWall != null)
            {
                var wall = drawable.instancedWall;
                props.SetFloat("_WallLength", wall.wallLength);
                props.SetVector("_CameraPos", cameraPosition);
                props.SetInt("_IsArch", 0);
                material.SetBuffer("instanced_wall_data", wall.instanceBuffer);
                props.SetTexture("_PathTex", pathTexture);

                Graphics.DrawMeshInstancedProcedural(
                    mesh,
                    0,
                    material,
                    drawable.bounds,
                    wall.instanceCount,
                    props
                );
                continue;
            }

            Graphics.DrawMesh(
                mesh,
                drawable.transformMatrix,
                material,
                0,
                mainCamera,
                0,
                props,
                ShadowCastingMode.On,
                true
            );
        }

        // Transparent pass
        Shader.SetGlobalInt("_DepthMask", 0);
        foreach (var drawable in transparentPass)
        {
            Mesh mesh = meshLibrary.GetMesh(drawable.meshHandle);
            Shader shader = shaderLibrary.GetShaderByHandle(drawable.shaderHandle);
            if (mesh == null || shader == null) continue;

            Material material = new Material(shader);
            MaterialPropertyBlock props = drawable.propertyBlock ?? new MaterialPropertyBlock();

            props.SetMatrix("_Model", drawable.transformMatrix);
            props.SetMatrix("_View", viewMatrix);
            props.SetMatrix("_Projection", projectionMatrix);
            props.SetTexture("_TerrainTex", terrainTexture);
            props.SetTexture("_PathTex", pathTexture);

            Graphics.DrawMesh(
                mesh,
                drawable.transformMatrix,
                material,
                0,
                mainCamera,
                0,
                props,
                ShadowCastingMode.On,
                true
            );
        }
        Shader.SetGlobalInt("_DepthMask", 1);
    }
}



/*
using UnityEngine;

public class RenderManager : MonoBehaviour
{
    // ----------------------------------------------------------------------------------------------------
    // Imports et D�clarations
    // ----------------------------------------------------------------------------------------------------

    // Rust: use std::ffi::CString;                                                                                        // Utilis� pour cr�er des cha�nes de caract�res C-style en Rust.
    // Rust: use std::ptr;                                                                                                 // Utilis� pour manipuler des pointeurs en Rust.
    // Rust: use bevy_ecs::prelude::World;                                                                                 // Utilis� pour acc�der au syst�me ECS (Entity-Component-System) de Bevy.
    // Rust: use bevy_input::mouse::MouseButton;                                                                           // Utilis� pour g�rer les entr�es de la souris dans Bevy.
    // Rust: use bevy_input::Input;                                                                                        // Utilis� pour g�rer les entr�es utilisateur dans Bevy.
    // Rust: use glutin::{window::Window, ContextWrapper, PossiblyCurrent};                                                // Utilis� pour cr�er et g�rer des fen�tres et des contextes OpenGL avec Glutin.
    // Rust: use crate::asset_libraries::{shader_library::AssetShaderLibrary, vao_library::AssetVAOLibrary, Handle};       // Importation des biblioth�ques d'assets pour les shaders et les VAO (Vertex Array Objects).
    // Rust: use crate::geometry::instanced_wall::InstancedWall;                                                           // Importation de la g�om�trie pour les murs instanci�s.
    // Rust: use crate::render::{camera::MainCamera, shader::{GlUniform, ShaderProgram}, vao::VAO};                        // Importation des composants de rendu, y compris la cam�ra principale, les shaders et les VAO.
    // Rust: use crate::resources::compute_path_mask::*;                                                                   // Importation des ressources pour le masque de chemin calcul�.
    // Rust: use crate::resources::curve_segments_pass::CURVE_BUFFER_SIZE;                                                 // Importation de la taille du tampon pour les segments de courbe.
    // Rust: use crate::resources::CurveSegmentsComputePass;                                                               // Importation du passage de calcul pour les segments de courbe.
    // Rust: use crate::systems::mode_manager::{BrushMode, EraseLayer};                                                    // Importation des modes de pinceau et des couches d'effacement.
    // Rust: use crate::window_events::WindowSize;                                                                         // Importation des �v�nements de fen�tre pour obtenir la taille de la fen�tre.
    // Rust: use crate::{components::*, TerrainData};                                                                      // Importation des composants et des donn�es de terrain.
    // Rust: use crate::{ComputeArchesIndirect, ComputePathMask, CursorRaycast};                                           // Importation des composants pour le calcul des arches, le masque de chemin et le raycasting du curseur.
    // Rust: use crate::utils::custom_macro::log_if_error;                                                                 // Importation d'une macro personnalis�e pour logger les erreurs.
    // Rust: use crate::components::{drawable::GLDrawMode, transform::Transform};                                          // Importation des composants pour le mode de dessin et les transformations.

    // ----------------------------------------------------------------------------------------------------
    // Fonction de Rendu
    // ----------------------------------------------------------------------------------------------------

    // Rust: pub fn render(ecs: &mut World, windowed_context: &mut ContextWrapper<PossiblyCurrent, Window>) {              // D�but de la fonction de rendu en Rust.
    // Rust:     puffin::profile_function!();                                                                              // Utilisation de Puffin pour profiler la fonction.
    // Rust:     let mut _img_unit = 0;                                                                                    // Initialisation d'une variable pour suivre l'unit� de texture active.

    // ----------------------------------------------------------------------------------------------------
    // Bloc Unsafe pour OpenGL
    // ----------------------------------------------------------------------------------------------------

    // Rust:     unsafe {                                                                                                  // D�but d'un bloc unsafe en Rust pour utiliser des appels OpenGL.
    // Rust:         gl::DepthMask(gl::TRUE);                                                                              // Active l'�criture dans le tampon de profondeur.

    // ----------------------------------------------------------------------------------------------------
    // R�cup�ration des Ressources
    // ----------------------------------------------------------------------------------------------------

    // Rust:         let indirect_test = ecs.get_resource::<ComputeArchesIndirect>().unwrap(); // R�cup�ration de la ressource pour le calcul des arches indirectes.
    // Rust:         let compute_curve_segments = ecs.get_resource::<CurveSegmentsComputePass>().unwrap(); // R�cup�ration de la ressource pour le calcul des segments de courbe.
    // Rust:         let path_mask = &ecs.get_resource::<ComputePathBlur>().unwrap().0; // R�cup�ration du masque de chemin flou.
    // Rust:         let assets_shader = ecs.get_resource::<AssetShaderLibrary>().unwrap(); // R�cup�ration de la biblioth�que de shaders.

    // ----------------------------------------------------------------------------------------------------
    // Calcul des Segments de Courbe
    // ----------------------------------------------------------------------------------------------------

    // Rust:         compute_curve_segments.reset_cmd_buffer(); // R�initialisation du tampon de commandes pour les segments de courbe.
    // Rust:         compute_curve_segments.reset_segments_buffer(); // R�initialisation du tampon de segments.
    // Rust:         compute_curve_segments.bind(assets_shader, path_mask.texture.id, PATH_MASK_WS_DIMS, _img_unit); // Liaison des ressources n�cessaires pour le calcul des segments de courbe.
    // Rust:         gl::DispatchCompute(CURVE_BUFFER_SIZE as u32, 1, 1); // Lancement du calcul des segments de courbe.
    // Rust:         gl::MemoryBarrier(gl::COMMAND_BARRIER_BIT | gl::SHADER_STORAGE_BARRIER_BIT); // Ajout d'une barri�re de m�moire pour s'assurer que les calculs sont termin�s avant de continuer.

    // ----------------------------------------------------------------------------------------------------
    // Calcul des Arches Indirectes
    // ----------------------------------------------------------------------------------------------------

    // Rust:         indirect_test.reset_draw_command_buffer(); // R�initialisation du tampon de commandes de dessin pour les arches indirectes.
    // Rust:         indirect_test.reset_transform_buffer(); // R�initialisation du tampon de transformations.
    // Rust:         indirect_test.bind(assets_shader, &compute_curve_segments.segments_buffer, path_mask.texture.id, PATH_MASK_WS_DIMS, _img_unit); // Liaison des ressources n�cessaires pour le passage de calcul des arches indirectes.
    // Rust:         gl::DispatchComputeIndirect(0); // Lancement du calcul indirect pour les arches.
    // Rust:         gl::MemoryBarrier(gl::COMMAND_BARRIER_BIT | gl::SHADER_STORAGE_BARRIER_BIT); // Ajout d'une barri�re de m�moire pour s'assurer que les calculs sont termin�s avant de continuer.

    // ----------------------------------------------------------------------------------------------------
    // Calcul du Masque de Chemin
    // ----------------------------------------------------------------------------------------------------

    // Rust:         let path_mask = &ecs.get_resource::<ComputePathMask>().unwrap().0; // R�cup�ration du masque de chemin.
    // Rust:         let path_blur = &ecs.get_resource::<ComputePathBlur>().unwrap().0; // R�cup�ration du masque de chemin flou.
    // Rust:         let mouse = ecs.get_resource::<CursorRaycast>().unwrap(); // R�cup�ration de la position du curseur.
    // Rust:         let mouse_button_input = ecs.get_resource::<Input<MouseButton>>().unwrap(); // R�cup�ration des entr�es de la souris.
    // Rust:         let _mode = ecs.get_resource::<BrushMode>().unwrap(); // R�cup�ration du mode de pinceau actuel.

    // ----------------------------------------------------------------------------------------------------
    // Mise � Jour du Shader en Fonction du Mode de Pinceau
    // ----------------------------------------------------------------------------------------------------

    // Rust:         if (matches!(_mode, BrushMode::Path) || matches!(_mode, BrushMode::Eraser(EraseLayer::All))) && mouse_button_input.pressed(MouseButton::Left) { // V�rification si le bouton gauche de la souris est press� et si le mode de pinceau est "Path".
    // Rust:             let shader = assets_shader.get(path_mask.compute_program).unwrap(); // R�cup�ration du shader pour le calcul du masque de chemin.
    // Rust:             gl::UseProgram(shader.id()); // Utilisation du shader.
    // Rust:             match _mode { BrushMode::Wall => panic!(), BrushMode::Path => { log_if_error!(shader.set_gl_uniform("is_additive", GlUniform::Bool(true))) }, BrushMode::Eraser(..) => { log_if_error!(shader.set_gl_uniform("is_additive", GlUniform::Bool(false))) } } // D�finition des uniformes du shader en fonction du mode de pinceau.
    // Rust:             let uniform_name = CString::new("img_output").unwrap(); // Cr�ation d'une cha�ne C pour le nom de l'uniforme.
    // Rust:             let tex_location = gl::GetUniformLocation(shader.id(), uniform_name.as_ptr() as *const i8); // R�cup�ration de l'emplacement de l'uniforme dans le shader.
    // Rust:             gl::Uniform1i(tex_location, _img_unit as i32); // D�finition de l'uniforme pour la texture de sortie.
    // Rust:             gl::BindImageTexture(_img_unit, path_mask.texture.id, 0, gl::FALSE, 0, gl::READ_WRITE, gl::RGBA32F); // Liaison de la texture pour le masque de chemin.
    // Rust:             log_if_error!(shader.set_gl_uniform("Mouse_Position", GlUniform::Vec3(mouse.0.to_array()))); // D�finition de l'uniforme pour la position de la souris.
    // Rust:             log_if_error!(shader.set_gl_uniform("path_mask_ws_dims", GlUniform::Vec2([20.0, 20.0]))); // D�finition de l'uniforme pour les dimensions du masque de chemin.
    // Rust:             gl::DispatchCompute(path_mask.texture.dims.0 as u32, path_mask.texture.dims.1 as u32, 1); // Lancement du calcul pour le masque de chemin.
    // Rust:             _img_unit += 1; // Incr�mentation de l'unit� de texture active.
    // Rust:         }
    // Rust:         gl::MemoryBarrier(gl::SHADER_IMAGE_ACCESS_BARRIER_BIT); // Ajout d'une barri�re de m�moire pour s'assurer que l'�criture de l'image est termin�e avant la lecture.

    // ----------------------------------------------------------------------------------------------------
    // Flou du Masque de Chemin
    // ----------------------------------------------------------------------------------------------------

    // Rust:         _img_unit = 0; // R�initialisation de l'unit� de texture active.
    // Rust:         let shader = assets_shader.get(path_blur.compute_program).unwrap(); // R�cup�ration du shader pour le flou du masque de chemin.
    // Rust:         gl::UseProgram(shader.id()); // Utilisation du shader.
    // Rust:         let uniform_name = CString::new("img_in").unwrap(); // Cr�ation d'une cha�ne C pour le nom de l'uniforme.
    // Rust:         let tex_location = gl::GetUniformLocation(shader.id(), uniform_name.as_ptr() as *const i8); // R�cup�ration de l'emplacement de l'uniforme dans le shader.
    // Rust:         gl::Uniform1i(tex_location, _img_unit as i32); // D�finition de l'uniforme pour la texture d'entr�e.
    // Rust:         gl::BindImageTexture(_img_unit, path_mask.texture.id, 0, gl::FALSE, 0, gl::READ_WRITE, gl::RGBA32F); // Liaison de la texture pour le masque de chemin.
    // Rust:         _img_unit += 1; // Incr�mentation de l'unit� de texture active.
    // Rust:         let uniform_name = CString::new("img_out").unwrap(); // Cr�ation d'une cha�ne C pour le nom de l'uniforme.
    // Rust:         let tex_location = gl::GetUniformLocation(shader.id(), uniform_name.as_ptr() as *const i8); // R�cup�ration de l'emplacement de l'uniforme dans le shader.
    // Rust:         gl::Uniform1i(tex_location, _img_unit as i32); // D�finition de l'uniforme pour la texture de sortie.
    // Rust:         gl::BindImageTexture(_img_unit, path_blur.texture.id, 0, gl::FALSE, 0, gl::READ_WRITE, gl::RGBA32F); // Liaison de la texture pour le masque de chemin flou.
    // Rust:         _img_unit += 1; // Incr�mentation de l'unit� de texture active.
    // Rust:         gl::DispatchCompute(path_mask.texture.dims.0 as u32, path_mask.texture.dims.1 as u32, 1); // Lancement du calcul pour le flou du masque de chemin.
    // Rust:         gl::MemoryBarrier(gl::SHADER_IMAGE_ACCESS_BARRIER_BIT); // Ajout d'une barri�re de m�moire pour s'assurer que l'�criture de l'image est termin�e avant la lecture.

    // ----------------------------------------------------------------------------------------------------
    // Pr�paration du Rendu Principal
    // ----------------------------------------------------------------------------------------------------

    // Rust:         let texture_buffer = path_blur.texture.id; // R�cup�ration de l'ID de la texture pour le masque de chemin flou.
    // Rust:         let texture_buffer_blur = path_blur.texture.id; // R�cup�ration de l'ID de la texture pour le masque de chemin flou.
    // Rust:         let (width, height) = ecs.get_resource::<WindowSize>().unwrap().try_into_i32(); // R�cup�ration de la taille de la fen�tre.
    // Rust:         gl::Viewport(0, 0, width, height); // D�finition de la zone de rendu.
    // Rust:         gl::BindFramebuffer(gl::FRAMEBUFFER, 0); // Liaison du framebuffer par d�faut.
    // Rust:         gl::ClearColor(0.120741, 0.120741, 0.120741, 1.0); // D�finition de la couleur de fond.
    // Rust:         gl::Clear(gl::COLOR_BUFFER_BIT | gl::DEPTH_BUFFER_BIT); // Effacement des tampons de couleur et de profondeur.
    // Rust:         let main_camera = ecs.get_resource::<MainCamera>().unwrap(); // R�cup�ration de la cam�ra principale.
    // Rust:         let view_transform = main_camera.camera.world_to_camera_view(); // R�cup�ration de la transformation de vue de la cam�ra.
    // Rust:         let camera_position = main_camera.camera.position(); // R�cup�ration de la position de la cam�ra.
    // Rust:         let projection_transform = main_camera.camera.perspective_projection; // R�cup�ration de la transformation de projection de la cam�ra.

    // ----------------------------------------------------------------------------------------------------
    // Rendu des Entit�s Dessinables
    // ----------------------------------------------------------------------------------------------------

    // Rust:         let mut query = ecs.query::<(&Handle<VAO>, &Handle<ShaderProgram>, &Transform, Option<&GLDrawMode>, Option<&InstancedWall>, Option<&DisplayTestMask>, Option<&TransparencyPass>, Option<&IndirectDraw>, Option<&RoadComponent>)>(); // R�cup�ration de toutes les entit�s dessinables.
    // Rust:         let assets_vao = ecs.get_resource::<AssetVAOLibrary>().unwrap(); // R�cup�ration de la biblioth�que de VAO.
    // Rust:         let mut transparent_pass = Vec::new(); // Initialisation d'une liste pour les objets transparents.

    // Rust:         for (vao_handle, shader_handle, model_transform, gl_draw_flag, instanced_wall, debug_display_path_mask, transparency, indirect_draw, road) in query.iter(ecs) { // It�ration sur les entit�s dessinables.
    // Rust:             let vao = assets_vao.get(*vao_handle).expect("Oops! This VAO handle is invalid"); // R�cup�ration du VAO.
    // Rust:             let shader = assets_shader.get(*shader_handle).expect("Oops! This Shader handle is invalid"); // R�cup�ration du shader.
    // Rust:             if transparency.is_some() { transparent_pass.push((vao, shader, model_transform)); continue; } // Si l'objet est transparent, on le stocke pour plus tard.
    // Rust:             shader.gl_use_program(); // Utilisation du shader.
    // Rust:             gl::ActiveTexture(gl::TEXTURE1); gl::BindTexture(gl::TEXTURE_2D, terrain_data.texture.id); shader.set_gl_uniform("terrain_texture", GlUniform::Int(1)); gl::ActiveTexture(gl::TEXTURE0); // Liaison de la texture de terrain.
    // Rust:             if debug_display_path_mask.is_some() { gl::BindTexture(gl::TEXTURE_2D, texture_buffer_blur); } // Si le masque de chemin doit �tre affich�, on lie la texture correspondante.
    // Rust:             if road.is_some() { gl::ActiveTexture(gl::TEXTURE0); gl::BindTexture(gl::TEXTURE_2D, texture_buffer); log_if_error!(shader.set_gl_uniform("path_texture", GlUniform::Int(0))); gl::ActiveTexture(gl::TEXTURE1); gl::BindTexture(gl::TEXTURE_2D, terrain_data.texture.id); log_if_error!(shader.set_gl_uniform("terrain_texture", GlUniform::Int(1))); gl::ActiveTexture(gl::TEXTURE0); } // Si l'objet est une route, on lie les textures n�cessaires.
    // Rust:             gl::BindVertexArray(vao.id()); // Liaison du VAO.
    // Rust:             for (name, transform) in &[("model", model_transform.compute_matrix().to_cols_array()), ("view", view_transform.to_cols_array()), ("projection", projection_transform.to_cols_array())] { let _result = shader.set_gl_uniform(name, GlUniform::Mat4(*transform)); } // D�finition des uniformes pour les transformations de mod�le, de vue et de projection.
    // Rust:             if indirect_draw.is_some() { log_if_error!(shader.set_gl_uniform("is_arch", GlUniform::Bool(true))); indirect_test.transforms_buffer.bind(&shader, "transforms_buffer"); gl::DrawElementsIndirect(gl::TRIANGLES, gl::UNSIGNED_INT, ptr::null()); continue; } // Si l'objet utilise un dessin indirect, on configure les uniformes et on lance le dessin.
    // Rust:             let mode = gl_draw_flag.map(|c| c.0).unwrap_or(gl::TRIANGLES); // D�termination du mode de dessin.
    // Rust:             if let Some(instanced_wall) = instanced_wall { log_if_error!(shader.set_gl_uniform("camera_position", GlUniform::Vec3(camera_position.to_array()))); log_if_error!(shader.set_gl_uniform("wall_length", GlUniform::Float(instanced_wall.wall_length))); log_if_error!(shader.set_gl_uniform("is_arch", GlUniform::Bool(false))); instanced_wall.instance_buffer.bind(shader, "instanced_wall_data"); gl::BindTexture(gl::TEXTURE_2D, texture_buffer); gl::DrawArraysInstanced(mode, 0, vao.indices_count as i32, instanced_wall.instance_buffer.instance_num as i32); } else { gl::DrawElements(mode, vao.indices_count as i32, gl::UNSIGNED_INT, ptr::null()); } // Si l'objet est un mur instanci�, on configure les uniformes et on lance le dessin instanci�.
    // Rust:         }

    // ----------------------------------------------------------------------------------------------------
    // Rendu des Objets Transparents
    // ----------------------------------------------------------------------------------------------------

    // Rust:         gl::DepthMask(gl::FALSE); // D�sactivation de l'�criture dans le tampon de profondeur pour les objets transparents.
    // Rust:         for (vao, shader, model_transform) in transparent_pass { // It�ration sur les objets transparents.
    // Rust:             shader.gl_use_program(); // Utilisation du shader.
    // Rust:             gl::ActiveTexture(gl::TEXTURE1); gl::BindTexture(gl::TEXTURE_2D, terrain_data.texture.id); log_if_error!(shader.set_gl_uniform("terrain_texture", GlUniform::Int(1))); gl::ActiveTexture(gl::TEXTURE0); // Liaison de la texture de terrain.
    // Rust:             gl::BindTexture(gl::TEXTURE_2D, texture_buffer); // Liaison de la texture pour le masque de chemin.
    // Rust:             gl::BindVertexArray(vao.id()); // Liaison du VAO.
    // Rust:             for (name, transform) in &[("model", model_transform.compute_matrix().to_cols_array()), ("view", view_transform.to_cols_array()), ("projection", projection_transform.to_cols_array())] { let _result = shader.set_gl_uniform(name, GlUniform::Mat4(*transform)); } // D�finition des uniformes pour les transformations de mod�le, de vue et de projection.
    // Rust:             gl::DrawElements(gl::TRIANGLES, vao.indices_count as i32, gl::UNSIGNED_INT, ptr::null()); // Lancement du dessin pour les objets transparents.
    // Rust:         }

    // ----------------------------------------------------------------------------------------------------
    // �change des Tampons
    // ----------------------------------------------------------------------------------------------------

    // Rust:         windowed_context.swap_buffers().unwrap(); // �change des tampons pour afficher le rendu.
    // Rust:     }
    // Rust: }
}

*/
